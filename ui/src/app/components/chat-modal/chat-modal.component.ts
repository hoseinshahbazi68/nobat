import { Component, OnDestroy, OnInit } from '@angular/core';
import { interval, of, Subscription } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ChatMessage as ApiChatMessage, ChatService } from '../../services/chat.service';

// Interface محلی برای نمایش در کامپوننت
interface ChatMessage extends ApiChatMessage {
  text?: string;
  sender?: 'user' | 'support';
  timestamp?: Date;
}

@Component({
  selector: 'app-chat-modal',
  standalone: false,
  templateUrl: './chat-modal.component.html',
  styleUrls: ['./chat-modal.component.scss']
})
export class ChatModalComponent implements OnInit, OnDestroy {
  messages: ChatMessage[] = [];
  newMessage: string = '';
  dialogRef: any = null;
  isTyping: boolean = false;
  isLoading: boolean = false;
  chatSessionId: string = '';
  phoneNumber: string = '';
  showPhoneInput: boolean = true;
  isSupportOnline: boolean = false;
  private pollingSubscription?: Subscription;
  private statusPollingSubscription?: Subscription;
  private currentUserId?: number;

  constructor(private chatService: ChatService) {
    // دریافت اطلاعات کاربر از localStorage
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        const user = JSON.parse(userStr);
        this.currentUserId = user.id;
        // اگر کاربر لاگین بود، شماره موبایلش را می‌گیریم
        if (user.phoneNumber) {
          this.phoneNumber = user.phoneNumber;
          this.showPhoneInput = false;
        }
      } catch (e) {
        console.error('Error parsing user data', e);
      }
    }
  }

  ngOnInit() {
    // اگر شماره موبایل وجود داشت، شناسه جلسه چت را ایجاد می‌کنیم
    if (this.phoneNumber) {
      this.initializeChat();
    }
  }

  initializeChat() {
    if (!this.phoneNumber || !this.isValidPhoneNumber(this.phoneNumber)) {
      alert('لطفاً شماره موبایل معتبر وارد کنید');
      return;
    }

    // ایجاد شناسه جلسه چت بر اساس شماره موبایل
    this.chatSessionId = this.chatService.generateChatSessionId(this.phoneNumber);
    this.showPhoneInput = false;

    // لاگ برای بررسی آدرس API (فقط در حالت development)
    if (!environment.production) {
      console.log('Chat API Base URL:', (this.chatService as any).baseUrl);
      console.log('Chat Session ID:', this.chatSessionId);
      console.log('Phone Number:', this.phoneNumber);
    }

    // بررسی وضعیت آنلاین پشتیبان
    this.checkSupportStatus();
    this.startStatusPolling();

    // بارگذاری پیام‌های قبلی
    this.loadMessages();

    // شروع polling برای دریافت پیام‌های جدید
    this.startPolling();
  }

  ngOnDestroy() {
    // توقف polling
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
    }
    if (this.statusPollingSubscription) {
      this.statusPollingSubscription.unsubscribe();
    }
  }

  isValidPhoneNumber(phone: string): boolean {
    // بررسی شماره موبایل ایرانی (09xxxxxxxxx)
    const phoneRegex = /^09\d{9}$/;
    return phoneRegex.test(phone);
  }

  checkSupportStatus() {
    this.chatService.getSupportStatus().subscribe({
      next: (status) => {
        this.isSupportOnline = status.isOnline;
        // اگر پشتیبان آنلاین نبود و پیام خوش‌آمدگویی هنوز اضافه نشده، پیام آفلاین اضافه می‌کنیم
        if (!this.isSupportOnline && this.messages.length === 0) {
          this.addOfflineMessage();
        }
      },
      error: (error) => {
        console.error('Error checking support status', error);
        // در صورت خطا، فرض می‌کنیم پشتیبان آنلاین نیست
        this.isSupportOnline = false;
      }
    });
  }

  startStatusPolling() {
    // هر 10 ثانیه یکبار وضعیت آنلاین را بررسی می‌کنیم
    this.statusPollingSubscription = interval(10000)
      .subscribe(() => {
        this.checkSupportStatus();
      });
  }

  loadMessages() {
    this.isLoading = true;
    // استفاده از endpoint جدید بر اساس PhoneNumber
    this.chatService.getMessagesByPhone(this.phoneNumber).subscribe({
      next: (messages) => {
        this.messages = messages
          .map(msg => ({
            ...msg,
            createdAt: typeof msg.createdAt === 'string' ? new Date(msg.createdAt) : msg.createdAt,
            text: msg.message,
            sender: msg.senderType.toLowerCase() as 'user' | 'support',
            timestamp: typeof msg.createdAt === 'string' ? new Date(msg.createdAt) : msg.createdAt,
            supportUserName: msg.supportUserName
          }))
          .sort((a, b) => {
            const timeA = a.timestamp instanceof Date ? a.timestamp.getTime() : new Date(a.timestamp).getTime();
            const timeB = b.timestamp instanceof Date ? b.timestamp.getTime() : new Date(b.timestamp).getTime();

            return timeA - timeB;
          });
        this.isLoading = false;
        this.scrollToBottom();

        // اگر پیامی وجود ندارد، بر اساس وضعیت آنلاین پیام مناسب اضافه می‌کنیم
        if (this.messages.length === 0) {
          if (this.isSupportOnline) {
            this.addWelcomeMessage();
          } else {
            this.addOfflineMessage();
          }
        }
      },
      error: (error) => {
        console.error('Error loading messages', error);
        this.isLoading = false;
        // در صورت خطا، پیام خوش‌آمدگویی نمایش می‌دهیم
        this.addWelcomeMessage();
      }
    });
  }

  startPolling() {
    // هر 3 ثانیه یکبار پیام‌های جدید را بررسی می‌کنیم
    this.pollingSubscription = interval(3000)
      .pipe(
        switchMap(() => this.chatService.getMessagesByPhone(this.phoneNumber)),
        catchError(error => {
          console.error('Error polling messages', error);
          return of([]);
        })
      )
      .subscribe(messages => {
        // حفظ welcome/offline message (با id=0) که در دیتابیس نیست
        const welcomeOfflineMessages = this.messages.filter(m =>
          m.id === 0 && (m.senderName === 'Support' || m.sender === 'support')
        );

        // تبدیل پیام‌های جدید از API (همه پیام‌ها از دیتابیس می‌آیند)
        const apiMessages = messages.map(msg => ({
          ...msg,
          createdAt: typeof msg.createdAt === 'string' ? new Date(msg.createdAt) : msg.createdAt,
          text: msg.message,
          sender: msg.senderType.toLowerCase() as 'user' | 'support',
          timestamp: typeof msg.createdAt === 'string' ? new Date(msg.createdAt) : msg.createdAt,
          supportUserName: msg.supportUserName
        }));

        // ترکیب welcome/offline messages با پیام‌های API
        const allMessages = [...welcomeOfflineMessages, ...apiMessages];

        // حذف تکراری‌ها بر اساس id
        const uniqueMessages = allMessages.filter((msg, index, self) => {
          if (msg.id === 0) {
            // برای welcome/offline messages، بر اساس message text بررسی می‌کنیم
            return index === self.findIndex(m => m.id === 0 && m.message === msg.message);
          }
          return index === self.findIndex(m => m.id === msg.id && m.id !== 0);
        });

        // مرتب‌سازی بر اساس timestamp
        // همه پیام‌ها در دیتابیس ذخیره می‌شوند، پس فقط بر اساس timestamp مرتب می‌کنیم
        uniqueMessages.sort((a, b) => {
          const timeA = a.timestamp instanceof Date
            ? a.timestamp.getTime()
            : (a.timestamp ? new Date(a.timestamp).getTime() : (a.createdAt instanceof Date ? a.createdAt.getTime() : new Date(a.createdAt).getTime()));
          const timeB = b.timestamp instanceof Date
            ? b.timestamp.getTime()
            : (b.timestamp ? new Date(b.timestamp).getTime() : (b.createdAt instanceof Date ? b.createdAt.getTime() : new Date(b.createdAt).getTime()));

          return timeA - timeB;
        });

        // به‌روزرسانی لیست پیام‌ها
        this.messages = uniqueMessages;
        this.scrollToBottom();
      });
  }

  addWelcomeMessage() {
    const welcomeMessage: ChatMessage = {
      id: 0,
      message: 'سلام! به سیستم نوبت‌دهی آنلاین خوش آمدید. چطور می‌توانم کمکتان کنم؟',
      senderType: 'Support',
      chatSessionId: this.chatSessionId,
      isRead: true,
      createdAt: new Date(),
      text: 'سلام! به سیستم نوبت‌دهی آنلاین خوش آمدید. چطور می‌توانم کمکتان کنم؟',
      sender: 'support',
      timestamp: new Date()
    };

    this.messages.push(welcomeMessage);
  }

  addOfflineMessage() {
    const offlineMessage: ChatMessage = {
      id: 0,
      message: 'افلاین هستم در اولین فرصت به شما پاسخ می دهم',
      senderType: 'Support',
      chatSessionId: this.chatSessionId,
      isRead: true,
      createdAt: new Date(),
      text: 'افلاین هستم در اولین فرصت به شما پاسخ می دهم',
      sender: 'support',
      timestamp: new Date()
    };

    this.messages.push(offlineMessage);
  }

  sendMessage() {
    if (!this.newMessage.trim() || this.isTyping) {
      return;
    }

    const messageText = this.newMessage.trim();
    this.newMessage = '';
    this.isTyping = true;

    // بررسی اینکه phoneNumber وجود دارد
    if (!this.phoneNumber || this.phoneNumber.trim() === '') {
      alert('لطفاً شماره موبایل را وارد کنید');
      this.isTyping = false;
      this.newMessage = messageText; // بازگرداندن پیام به input
      return;
    }

    // ارسال پیام به API
    this.chatService.sendMessage({
      message: messageText,
      phoneNumber: this.phoneNumber.trim()
    }).subscribe({
      next: (response) => {
        // همه پیام‌ها در دیتابیس ذخیره می‌شوند
        // بعد از ارسال پیام، از دیتابیس دوباره پیام‌ها را می‌خوانیم
        this.loadMessages();

        this.isTyping = false;
        this.scrollToBottom();
      },
      error: (error) => {
        console.error('Error sending message', error);

        // بررسی نوع خطا و نمایش پیام مناسب
        let errorMessage = 'خطا در ارسال پیام. لطفاً دوباره تلاش کنید.';
        if (error?.error?.message) {
          errorMessage = error.error.message;
        } else if (error?.message) {
          errorMessage = error.message;
        }

        this.isTyping = false;
        this.newMessage = messageText; // بازگرداندن پیام به input

        // نمایش پیام خطا به کاربر
        alert(errorMessage);
      }
    });
  }

  scrollToBottom() {
    setTimeout(() => {
      const chatMessages = document.querySelector('.chat-messages');
      if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
      }
    }, 100);
  }

  onKeyPress(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  closeChat() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }

  formatTime(date: Date | string): string {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    const hours = dateObj.getHours().toString().padStart(2, '0');
    const minutes = dateObj.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  }
}
