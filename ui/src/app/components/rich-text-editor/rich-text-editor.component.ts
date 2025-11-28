import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

declare var Quill: any;

@Component({
  selector: 'app-rich-text-editor',
  template: `
    <div class="rich-text-editor-container">
      <div #editorContainer class="editor-container"></div>
    </div>
  `,
  styles: [`
    .rich-text-editor-container {
      width: 100%;
    }

    .editor-container {
      min-height: 300px;
      background: var(--bg-secondary, #fff);
      border: 2px solid var(--border-color, #e5e7eb);
      border-radius: var(--radius-md, 8px);
      direction: rtl;
    }

    .editor-container :global(.ql-container) {
      font-family: 'Vazirmatn', 'Tahoma', sans-serif;
      font-size: 1rem;
      direction: rtl;
      text-align: right;
      min-height: 250px;
    }

    .editor-container :global(.ql-editor) {
      direction: rtl;
      text-align: right;
    }

    .editor-container :global(.ql-toolbar) {
      border-top-right-radius: var(--radius-md, 8px);
      border-top-left-radius: var(--radius-md, 8px);
      border-bottom: 2px solid var(--border-color, #e5e7eb);
      direction: rtl;
    }

    .editor-container :global(.ql-toolbar .ql-formats) {
      margin-left: 15px;
    }

    .editor-container :global(.ql-snow .ql-picker-label) {
      direction: rtl;
    }

    .editor-container :global(.ql-snow .ql-picker-options) {
      direction: rtl;
    }
  `]
})
export class RichTextEditorComponent implements OnInit {
  @ViewChild('editorContainer', { static: true }) editorContainer!: ElementRef;
  @Input() value: string = '';
  @Input() placeholder: string = 'متن خود را وارد کنید...';
  @Output() valueChange = new EventEmitter<string>();
  @Output() contentChange = new EventEmitter<string>();

  private quill: any;
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.initQuill();
  }

  private initQuill() {
    const toolbarOptions = [
      [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
      [{ 'font': [] }],
      [{ 'size': ['small', false, 'large', 'huge'] }],
      ['bold', 'italic', 'underline', 'strike'],
      [{ 'color': [] }, { 'background': [] }],
      [{ 'script': 'sub'}, { 'script': 'super' }],
      [{ 'list': 'ordered'}, { 'list': 'bullet' }],
      [{ 'indent': '-1'}, { 'indent': '+1' }],
      [{ 'direction': 'rtl' }],
      [{ 'align': [] }],
      ['link', 'image', 'video'],
      ['blockquote', 'code-block'],
      ['clean']
    ];

    this.quill = new Quill(this.editorContainer.nativeElement, {
      theme: 'snow',
      placeholder: this.placeholder,
      modules: {
        toolbar: {
          container: toolbarOptions,
          handlers: {
            image: () => this.handleImageUpload()
          }
        }
      },
      direction: 'rtl'
    });

    // تنظیم مقدار اولیه
    if (this.value) {
      this.quill.root.innerHTML = this.value;
    }

    // گوش دادن به تغییرات
    this.quill.on('text-change', () => {
      const content = this.quill.root.innerHTML;
      this.valueChange.emit(content);
      this.contentChange.emit(content);
    });
  }

  private handleImageUpload() {
    const input = document.createElement('input');
    input.setAttribute('type', 'file');
    input.setAttribute('accept', 'image/*');
    input.click();

    input.onchange = () => {
      const file = input.files?.[0];
      if (!file) return;

      // بررسی اندازه فایل (حداکثر 5 مگابایت)
      if (file.size > 5 * 1024 * 1024) {
        alert('حجم فایل نباید بیشتر از 5 مگابایت باشد');
        return;
      }

      // نمایش loading
      const range = this.quill.getSelection(true);
      const index = range ? range.index : 0;
      this.quill.insertText(index, 'در حال آپلود عکس...', 'user');
      this.quill.setSelection(index + 20);

      // آپلود عکس
      this.uploadImage(file).subscribe({
        next: (response: any) => {
          // حذف متن loading
          this.quill.deleteText(index, 20);

          // درج عکس
          const imageUrl = response.url || response.data?.url || response;
          if (imageUrl) {
            this.quill.insertEmbed(index, 'image', imageUrl);
            this.quill.setSelection(index + 1);
          } else {
            alert('خطا در آپلود عکس');
          }
        },
        error: (error) => {
          // حذف متن loading
          const currentIndex = this.quill.getSelection()?.index || index;
          this.quill.deleteText(index, 20);
          alert('خطا در آپلود عکس: ' + (error.error?.message || error.message || 'خطای نامشخص'));
        }
      });
    };
  }

  private uploadImage(file: File) {
    const formData = new FormData();
    formData.append('file', file);

    // استفاده از endpoint آپلود عکس پروفایل
    // در آینده می‌توانید یک endpoint عمومی برای آپلود عکس اضافه کنید
    return this.http.post<any>(`${this.apiUrl}/Auth/me/profile-picture`, formData).pipe(
      map((response: any) => {
        // استخراج URL عکس از response
        if (response && response.data) {
          const user = response.data;
          const profilePicture = user.profilePicture;
          if (profilePicture) {
            // اگر URL کامل است، مستقیماً استفاده کن
            if (profilePicture.startsWith('http://') || profilePicture.startsWith('https://')) {
              return { url: profilePicture };
            }
            // اگر URL نسبی است، base URL را اضافه کن
            const baseUrl = this.apiUrl.replace('/api/v1', '').replace('/api', '');
            return { url: profilePicture.startsWith('/') ? `${baseUrl}${profilePicture}` : `${baseUrl}/${profilePicture}` };
          }
        }
        throw new Error('خطا در آپلود عکس');
      })
    );
  }

  getContent(): string {
    return this.quill ? this.quill.root.innerHTML : '';
  }

  setContent(content: string) {
    if (this.quill) {
      this.quill.root.innerHTML = content;
    }
  }

  clear() {
    if (this.quill) {
      this.quill.setText('');
    }
  }
}
