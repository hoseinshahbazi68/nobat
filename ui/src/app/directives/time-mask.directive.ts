import { Directive, ElementRef, HostListener, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Directive({
  selector: '[appTimeMask]',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TimeMaskDirective),
      multi: true
    }
  ]
})
export class TimeMaskDirective implements ControlValueAccessor {
  private onChange = (value: string) => {};
  private onTouched = () => {};

  constructor(private el: ElementRef<HTMLInputElement>) {}

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let value = input.value;

    // حذف همه کاراکترهای غیر عددی به جز :
    let numbers = value.replace(/[^\d]/g, '');

    // محدود کردن به 4 رقم
    if (numbers.length > 4) {
      numbers = numbers.substring(0, 4);
    }

    // فرمت کردن به HH:MM
    let formattedValue = '';
    if (numbers.length === 0) {
      formattedValue = '';
    } else if (numbers.length <= 2) {
      formattedValue = numbers;
    } else {
      formattedValue = numbers.substring(0, 2) + ':' + numbers.substring(2, 4);
    }

    // اعتبارسنجی ساعت و دقیقه
    if (formattedValue.includes(':')) {
      const [hours, minutes] = formattedValue.split(':');

      if (hours) {
        const h = parseInt(hours);
        if (h > 23) {
          formattedValue = '23' + (minutes ? ':' + minutes : '');
        } else if (hours.length === 2 && h < 10) {
          formattedValue = '0' + h + (minutes ? ':' + minutes : '');
        }
      }

      if (minutes) {
        const m = parseInt(minutes);
        if (m > 59) {
          const h = hours || '00';
          formattedValue = h + ':59';
        } else if (minutes.length === 2 && m < 10) {
          formattedValue = (hours || '00') + ':0' + m;
        }
      }
    }

    input.value = formattedValue;
    this.onChange(formattedValue);
  }

  @HostListener('blur', ['$event'])
  onBlur(event: Event): void {
    const input = event.target as HTMLInputElement;
    let value = input.value.trim();

    if (!value) {
      this.onTouched();
      return;
    }

    // حذف کاراکترهای غیر عددی به جز :
    let numbers = value.replace(/[^\d]/g, '');

    // تکمیل فرمت در صورت نیاز
    if (numbers.length === 0) {
      input.value = '';
      this.onTouched();
      this.onChange('');
      return;
    }

    let formattedValue = '';
    if (numbers.length === 1) {
      formattedValue = '0' + numbers + ':00';
    } else if (numbers.length === 2) {
      formattedValue = numbers + ':00';
    } else if (numbers.length === 3) {
      formattedValue = numbers.substring(0, 2) + ':0' + numbers.substring(2);
    } else {
      formattedValue = numbers.substring(0, 2) + ':' + numbers.substring(2, 4);
    }

    // اعتبارسنجی نهایی
    const [hours, minutes] = formattedValue.split(':');
    if (hours) {
      const h = parseInt(hours);
      if (h > 23) {
        formattedValue = '23:' + (minutes || '00');
      } else {
        formattedValue = String(h).padStart(2, '0') + ':' + (minutes || '00');
      }
    }

    if (minutes) {
      const m = parseInt(minutes);
      if (m > 59) {
        const h = hours || '00';
        formattedValue = h + ':59';
      } else {
        const h = hours || '00';
        formattedValue = h + ':' + String(m).padStart(2, '0');
      }
    }

    input.value = formattedValue;
    this.onTouched();
    this.onChange(formattedValue);
  }

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    const input = event.target as HTMLInputElement;

    // اجازه به Backspace, Delete, Tab, Escape, Enter
    if ([8, 9, 27, 13, 46].indexOf(event.keyCode) !== -1 ||
      // اجازه به Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
      (event.keyCode === 65 && event.ctrlKey === true) ||
      (event.keyCode === 67 && event.ctrlKey === true) ||
      (event.keyCode === 86 && event.ctrlKey === true) ||
      (event.keyCode === 88 && event.ctrlKey === true) ||
      // اجازه به Home, End, Left, Right
      (event.keyCode >= 35 && event.keyCode <= 39)) {
      return;
    }

    // فقط اعداد مجاز
    if ((event.shiftKey || (event.keyCode < 48 || event.keyCode > 57)) && (event.keyCode < 96 || event.keyCode > 105)) {
      event.preventDefault();
    }
  }

  @HostListener('paste', ['$event'])
  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pastedText = event.clipboardData?.getData('text') || '';
    const numbers = pastedText.replace(/[^\d]/g, '').substring(0, 4);

    let formattedValue = numbers;
    if (numbers.length > 2) {
      formattedValue = numbers.substring(0, 2) + ':' + numbers.substring(2);
    }

    const input = event.target as HTMLInputElement;
    input.value = formattedValue;
    this.onChange(formattedValue);
  }

  writeValue(value: string): void {
    if (value) {
      // تبدیل فرمت HH:MM از مقدار
      let formattedValue = value;
      if (!formattedValue.includes(':')) {
        if (formattedValue.length === 4) {
          formattedValue = formattedValue.substring(0, 2) + ':' + formattedValue.substring(2);
        } else if (formattedValue.length === 3) {
          formattedValue = '0' + formattedValue.substring(0, 1) + ':' + formattedValue.substring(1);
        }
      }
      this.el.nativeElement.value = formattedValue;
    } else {
      this.el.nativeElement.value = '';
    }
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.el.nativeElement.disabled = isDisabled;
  }
}
