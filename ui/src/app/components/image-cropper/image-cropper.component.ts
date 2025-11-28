import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';

export interface ImageCropperData {
  imageFile: File;
  aspectRatio?: number; // نسبت ابعاد (مثلاً 1 برای مربع)
  maxWidth?: number; // حداکثر عرض
  maxHeight?: number; // حداکثر ارتفاع
  quality?: number; // کیفیت فشرده‌سازی (0-1)
  maxSizeKB?: number; // حداکثر حجم به کیلوبایت
}

@Component({
  selector: 'app-image-cropper',
  template: `
    <div class="cropper-dialog-container">
      <div class="cropper-dialog">
        <div class="cropper-header">
          <h2>کراپ و فشرده‌سازی عکس</h2>
        </div>
        <div class="cropper-content">
          <div class="cropper-wrapper" *ngIf="imageSrc">
            <div class="canvas-container">
              <canvas #canvasElement
                      [style.width.px]="canvasWidth"
                      [style.height.px]="canvasHeight"
                      (mousedown)="onMouseDown($event)"
                      (mousemove)="onMouseMove($event)"
                      (mouseup)="onMouseUp($event)"
                      (mouseleave)="onMouseUp($event)"></canvas>
              <div class="crop-overlay"
                   [style.left.px]="cropX"
                   [style.top.px]="cropY"
                   [style.width.px]="cropWidth"
                   [style.height.px]="cropHeight"
                   *ngIf="imageSrc">
                <div class="crop-handle crop-handle-nw" (mousedown)="onResizeStart($event, 'nw')"></div>
                <div class="crop-handle crop-handle-ne" (mousedown)="onResizeStart($event, 'ne')"></div>
                <div class="crop-handle crop-handle-sw" (mousedown)="onResizeStart($event, 'sw')"></div>
                <div class="crop-handle crop-handle-se" (mousedown)="onResizeStart($event, 'se')"></div>
              </div>
            </div>
          </div>
          <div class="preview-wrapper" *ngIf="previewSrc">
            <h3>پیش‌نمایش:</h3>
            <img [src]="previewSrc" alt="Preview" class="preview-image">
            <div class="file-info">
              <span>حجم: {{ fileSizeKB }} KB</span>
              <span>ابعاد: {{ previewWidth }} × {{ previewHeight }} px</span>
            </div>
          </div>
        </div>
        <div class="cropper-actions">
          <button type="button" class="btn btn-secondary" (click)="onCancel()">انصراف</button>
          <button type="button" class="btn btn-primary" (click)="onCrop()" [disabled]="!previewSrc">ذخیره</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .cropper-dialog-container {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.7);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
      animation: fadeIn 0.3s;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    .cropper-dialog {
      background: white;
      border-radius: 12px;
      max-width: 90vw;
      max-height: 90vh;
      display: flex;
      flex-direction: column;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
      animation: slideUp 0.3s;
    }

    @keyframes slideUp {
      from {
        transform: translateY(20px);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }

    .cropper-header {
      padding: 20px 24px;
      border-bottom: 1px solid #e5e7eb;
    }

    .cropper-header h2 {
      margin: 0;
      font-size: 1.5rem;
      font-weight: 700;
      color: #1f2937;
    }

    .cropper-content {
      padding: 24px;
      display: flex;
      gap: 24px;
      flex: 1;
      overflow: auto;
      min-height: 400px;
    }

    .cropper-wrapper {
      position: relative;
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f3f4f6;
      border-radius: 8px;
      overflow: hidden;
    }

    canvas {
      max-width: 100%;
      max-height: 70vh;
      cursor: move;
      border: 2px solid #3b82f6;
    }

    .canvas-container {
      position: relative;
      display: inline-block;
    }

    .crop-overlay {
      position: absolute;
      border: 2px solid #3b82f6;
      background: rgba(59, 130, 246, 0.1);
      pointer-events: none;
      box-sizing: border-box;
    }

    .crop-handle {
      position: absolute;
      width: 12px;
      height: 12px;
      background: #3b82f6;
      border: 2px solid white;
      border-radius: 50%;
      pointer-events: all;
      cursor: pointer;
    }

    .crop-handle-nw {
      top: -6px;
      left: -6px;
      cursor: nw-resize;
    }

    .crop-handle-ne {
      top: -6px;
      right: -6px;
      cursor: ne-resize;
    }

    .crop-handle-sw {
      bottom: -6px;
      left: -6px;
      cursor: sw-resize;
    }

    .crop-handle-se {
      bottom: -6px;
      right: -6px;
      cursor: se-resize;
    }

    .preview-wrapper {
      width: 300px;
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .preview-wrapper h3 {
      margin: 0;
      font-size: 1rem;
      font-weight: 600;
      color: #1f2937;
    }

    .preview-image {
      width: 100%;
      height: auto;
      border-radius: 8px;
      border: 2px solid #e5e7eb;
    }

    .file-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
      font-size: 0.875rem;
      color: #6b7280;
    }

    .cropper-actions {
      padding: 16px 24px;
      border-top: 1px solid #e5e7eb;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 8px;
      font-weight: 600;
      font-size: 0.95rem;
      cursor: pointer;
      transition: all 0.2s;
      font-family: 'Vazirmatn', sans-serif;
    }

    .btn-primary {
      background: linear-gradient(135deg, #06b6d4 0%, #3b82f6 100%);
      color: white;
      box-shadow: 0 4px 12px rgba(6, 182, 212, 0.3);
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(6, 182, 212, 0.4);
    }

    .btn-primary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-secondary {
      background: #f3f4f6;
      color: #1f2937;
      border: 2px solid #e5e7eb;
    }

    .btn-secondary:hover {
      background: #e5e7eb;
      border-color: #d1d5db;
    }

    @media (max-width: 768px) {
      .cropper-content {
        flex-direction: column;
      }

      .preview-wrapper {
        width: 100%;
      }
    }
  `]
})
export class ImageCropperComponent implements OnInit, AfterViewInit {
  @ViewChild('canvasElement', { static: false }) canvasElement!: ElementRef<HTMLCanvasElement>;

  data: ImageCropperData | null = null;
  dialogRef: any = null;

  imageSrc: string | null = null;
  canvasWidth = 600;
  canvasHeight = 400;
  cropX = 50;
  cropY = 50;
  cropWidth = 200;
  cropHeight = 200;
  isDragging = false;
  isResizing = false;
  resizeDirection = '';
  dragStartX = 0;
  dragStartY = 0;
  previewSrc: string | null = null;
  fileSizeKB = 0;
  previewWidth = 0;
  previewHeight = 0;

  private ctx: CanvasRenderingContext2D | null = null;
  private image: HTMLImageElement | null = null;

  ngOnInit() {
    if (this.data?.imageFile) {
      // کمی تاخیر برای اطمینان از render شدن canvas
      setTimeout(() => {
        if (this.data?.imageFile) {
          this.loadImage(this.data.imageFile);
        }
      }, 100);
    }
  }

  ngAfterViewInit() {
    if (this.data?.imageFile && this.canvasElement) {
      this.loadImage(this.data.imageFile);
    }
  }

  loadImage(file: File) {
    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.imageSrc = e.target.result;
      const img = new Image();
      img.onload = () => {
        this.image = img;
        this.setupCanvas(img);
      };
      img.src = e.target.result;
    };
    reader.readAsDataURL(file);
  }

  setupCanvas(img: HTMLImageElement) {
    if (!this.canvasElement?.nativeElement) return;
    const canvas = this.canvasElement.nativeElement;

    this.ctx = canvas.getContext('2d');
    if (!this.ctx) return;

    // محاسبه ابعاد canvas
    const maxWidth = this.data?.maxWidth || 800;
    const maxHeight = this.data?.maxHeight || 600;
    const aspectRatio = img.width / img.height;

    if (img.width > img.height) {
      this.canvasWidth = Math.min(img.width, maxWidth);
      this.canvasHeight = this.canvasWidth / aspectRatio;
    } else {
      this.canvasHeight = Math.min(img.height, maxHeight);
      this.canvasWidth = this.canvasHeight * aspectRatio;
    }

    canvas.width = this.canvasWidth;
    canvas.height = this.canvasHeight;

    // رسم تصویر
    this.ctx.drawImage(img, 0, 0, this.canvasWidth, this.canvasHeight);

    // تنظیم ابعاد اولیه کراپ
    const aspectRatioCrop = this.data?.aspectRatio || 1;
    this.cropWidth = Math.min(this.canvasWidth * 0.6, 300);
    this.cropHeight = this.cropWidth / aspectRatioCrop;
    this.cropX = (this.canvasWidth - this.cropWidth) / 2;
    this.cropY = (this.canvasHeight - this.cropHeight) / 2;

    this.updatePreview();
  }

  onMouseDown(event: MouseEvent) {
    // اگر روی handle کلیک شده، resize شروع می‌شود
    if ((event.target as HTMLElement).classList.contains('crop-handle')) {
      return; // handle خودش event را مدیریت می‌کند
    }

    if (!this.canvasElement?.nativeElement) return;
    const canvas = this.canvasElement.nativeElement;

    const rect = canvas.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;

    // بررسی اینکه آیا کلیک داخل ناحیه کراپ است
    if (x >= this.cropX && x <= this.cropX + this.cropWidth &&
      y >= this.cropY && y <= this.cropY + this.cropHeight) {
      this.isDragging = true;
      this.dragStartX = x - this.cropX;
      this.dragStartY = y - this.cropY;
      event.preventDefault();
    }
  }

  onResizeStart(event: MouseEvent, direction: string) {
    event.stopPropagation();
    this.isResizing = true;
    this.resizeDirection = direction;
    this.dragStartX = event.clientX;
    this.dragStartY = event.clientY;
  }

  onMouseMove(event: MouseEvent) {
    if (this.isResizing) {
      if (!this.canvasElement?.nativeElement || !this.image) return;
      const canvas = this.canvasElement.nativeElement;

      const rect = canvas.getBoundingClientRect();
      if (!this.image) return;
      const deltaX = (event.clientX - this.dragStartX) * (this.image.width / this.canvasWidth);
      const deltaY = (event.clientY - this.dragStartY) * (this.image.height / this.canvasHeight);

      const aspectRatio = this.data?.aspectRatio || 1;
      let newWidth = this.cropWidth;
      let newHeight = this.cropHeight;
      let newX = this.cropX;
      let newY = this.cropY;

      if (this.resizeDirection.includes('e')) {
        newWidth = Math.max(50, Math.min(this.cropWidth + deltaX, this.canvasWidth - this.cropX));
        newHeight = newWidth / aspectRatio;
      }
      if (this.resizeDirection.includes('w')) {
        newWidth = Math.max(50, Math.min(this.cropWidth - deltaX, this.cropX + this.cropWidth));
        newHeight = newWidth / aspectRatio;
        newX = this.cropX + this.cropWidth - newWidth;
      }
      if (this.resizeDirection.includes('s')) {
        newHeight = Math.max(50, Math.min(this.cropHeight + deltaY, this.canvasHeight - this.cropY));
        if (!this.resizeDirection.includes('e') && !this.resizeDirection.includes('w')) {
          newWidth = newHeight * aspectRatio;
        }
      }
      if (this.resizeDirection.includes('n')) {
        newHeight = Math.max(50, Math.min(this.cropHeight - deltaY, this.cropY + this.cropHeight));
        if (!this.resizeDirection.includes('e') && !this.resizeDirection.includes('w')) {
          newWidth = newHeight * aspectRatio;
        }
        newY = this.cropY + this.cropHeight - newHeight;
      }

      // بررسی محدودیت‌ها
      if (newX >= 0 && newX + newWidth <= this.canvasWidth &&
        newY >= 0 && newY + newHeight <= this.canvasHeight) {
        this.cropX = newX;
        this.cropY = newY;
        this.cropWidth = newWidth;
        this.cropHeight = newHeight;
        this.dragStartX = event.clientX;
        this.dragStartY = event.clientY;
        this.updatePreview();
      }
    } else if (this.isDragging) {
      if (!this.canvasElement?.nativeElement) return;
      const canvas = this.canvasElement.nativeElement;

      const rect = canvas.getBoundingClientRect();
      const x = event.clientX - rect.left;
      const y = event.clientY - rect.top;

      this.cropX = Math.max(0, Math.min(x - this.dragStartX, this.canvasWidth - this.cropWidth));
      this.cropY = Math.max(0, Math.min(y - this.dragStartY, this.canvasHeight - this.cropHeight));

      this.updatePreview();
    }
  }

  onMouseUp(event?: MouseEvent) {
    this.isDragging = false;
    this.isResizing = false;
    this.resizeDirection = '';
  }

  updatePreview() {
    if (!this.image || !this.ctx) return;

    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // محاسبه ابعاد واقعی کراپ
    const scaleX = this.image.width / this.canvasWidth;
    const scaleY = this.image.height / this.canvasHeight;

    const cropWidth = this.cropWidth * scaleX;
    const cropHeight = this.cropHeight * scaleY;
    const cropX = this.cropX * scaleX;
    const cropY = this.cropY * scaleY;

    // تنظیم ابعاد نهایی
    const maxWidth = this.data?.maxWidth || 800;
    const maxHeight = this.data?.maxHeight || 600;
    let finalWidth = cropWidth;
    let finalHeight = cropHeight;

    if (finalWidth > maxWidth) {
      finalHeight = (finalHeight * maxWidth) / finalWidth;
      finalWidth = maxWidth;
    }
    if (finalHeight > maxHeight) {
      finalWidth = (finalWidth * maxHeight) / finalHeight;
      finalHeight = maxHeight;
    }

    canvas.width = finalWidth;
    canvas.height = finalHeight;

    // کراپ و رسم
    ctx.drawImage(
      this.image,
      cropX, cropY, cropWidth, cropHeight,
      0, 0, finalWidth, finalHeight
    );

    // فشرده‌سازی
    const quality = this.data?.quality || 0.8;
    let dataUrl = canvas.toDataURL('image/jpeg', quality);
    let sizeKB = this.getImageSizeKB(dataUrl);

    // اگر حجم بیشتر از حد مجاز است، کیفیت را کاهش بده
    const maxSizeKB = this.data?.maxSizeKB || 200;
    let currentQuality = quality;
    while (sizeKB > maxSizeKB && currentQuality > 0.1) {
      currentQuality -= 0.1;
      dataUrl = canvas.toDataURL('image/jpeg', currentQuality);
      sizeKB = this.getImageSizeKB(dataUrl);
    }

    this.previewSrc = dataUrl;
    this.fileSizeKB = Math.round(sizeKB);
    this.previewWidth = Math.round(finalWidth);
    this.previewHeight = Math.round(finalHeight);
  }

  getImageSizeKB(dataUrl: string): number {
    const base64 = dataUrl.split(',')[1];
    const binary = atob(base64);
    return binary.length / 1024;
  }

  onCrop() {
    if (!this.previewSrc) return;

    // تبدیل data URL به File
    this.dataURLtoFile(this.previewSrc, this.data?.imageFile.name || 'cropped-image.jpg').then(file => {
      if (this.dialogRef) {
        this.dialogRef.close(file);
      }
    });
  }

  dataURLtoFile(dataUrl: string, filename: string): Promise<File> {
    return fetch(dataUrl)
      .then(res => res.blob())
      .then(blob => new File([blob], filename, { type: 'image/jpeg' }));
  }

  onCancel() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }
}
