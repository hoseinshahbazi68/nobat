import { Component, OnInit, OnDestroy } from '@angular/core';
import { ChatService, ChatMessage as ApiChatMessage, ChatSession } from '../../services/chat.service';
import { SnackbarService } from '../../services/snackbar.service';
import { interval, Subscription } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

// Interface محلی برای نمایش در کامپوننت
interface ChatMessage extends ApiChatMessage {
  text?: string;
  sender?: 'user' | 'support';
  timestamp?: Date;
}

@Component({
  selector: 'app-chat-support',
  templateUrl: './chat-support.component.html',
  styleUrls: ['./chat-support.component.scss']
})
export class ChatSupportComponent implements OnInit, OnDestroy {
  sessions: ChatSession[] = [];
  selectedSession: ChatSession | null = null;
  messages: ChatMessage[] = [];
  newMessage: string = '';
  searchPhoneNumber: string = '';
  isLoading: boolean = false;
  isLoadingMessages: boolean = false;
  private pollingSubscription?: Subscription;
  private sessionPollingSubscription?: Subscription;

  constructor(
    private chatService: ChatService,
    private snackbarService: SnackbarService
  ) { }

  ngOnInit() {
    this.loadSessions();
    this.startSessionPolling();
  }

  ngOnDestroy() {
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
    }
    if (this.sessionPollingSubscription) {
      this.sessionPollingSubscription.unsubscribe();
    }
  }

  loadSessions() {
    this.isLoading = true;
    const phoneNumber = this.searchPhoneNumber.trim() || undefined;
    this.chatService.getActiveSessions(phoneNumber).subscribe({
      next: (sessions) => {
        this.sessions = sessions;
        this.isLoading = false;

        // اگر جلسه‌ای انتخاب شده بود، اطلاعات آن را به‌روزرسانی می‌کنیم
        if (this.selectedSession) {
          const updatedSession = sessions.find(s => s.chatSessionId === this.selectedSession!.chatSessionId);
          if (updatedSession) {
            this.selectedSession = updatedSession;
          }
        }
      },
      error: (error) => {
        console.error('Error loading sessions', error);
        this.snackbarService.error('خطا در بارگذاری جلسه‌های چت', 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  startSessionPolling() {
    // هر 5 ثانیه یکبار لیست جلسه‌ها را به‌روزرسانی می‌کنیم
    this.sessionPollingSubscription = interval(5000)
      .pipe(
        switchMap(() => {
          const phoneNumber = this.searchPhoneNumber.trim() || undefined;
          return this.chatService.getActiveSessions(phoneNumber);
        }),
        catchError(error => {
          console.error('Error polling sessions', error);
          return of([]);
        })
      )
      .subscribe(sessions => {
        this.sessions = sessions;

        // به‌روزرسانی جلسه انتخاب شده
        if (this.selectedSession) {
          const updatedSession = sessions.find(s => s.chatSessionId === this.selectedSession!.chatSessionId);
          if (updatedSession) {
            this.selectedSession = updatedSession;
          }
        }
      });
  }

  selectSession(session: ChatSession) {
    this.selectedSession = session;
    if (session.phoneNumber) {
      this.loadMessages(session.phoneNumber);
      this.startMessagePolling(session.phoneNumber);
    } else {
      this.loadMessages(session.chatSessionId);
      this.startMessagePolling(session.chatSessionId);
    }

    // علامت‌گذاری پیام‌ها به عنوان خوانده شده
    this.chatService.markAsRead(session.chatSessionId).subscribe({
      error: (error) => console.error('Error marking as read', error)
    });
  }

  loadMessages(identifier: string) {
    this.isLoadingMessages = true;
    // اگر identifier یک شماره موبایل است (شروع با 09)، از endpoint مخصوص پشتیبان استفاده می‌کنیم
    const isPhoneNumber = identifier.startsWith('09') && identifier.length === 11;
    const observable = isPhoneNumber
      ? this.chatService.getMessagesByPhoneForSupport(identifier) // استفاده از endpoint مخصوص پشتیبان برای ارسال پیام خوش‌آمدگویی
      : this.chatService.getMessages(identifier);

    observable.subscribe({
      next: (messages) => {
        this.messages = messages
          .map(msg => {
            const createdAt = typeof msg.createdAt === 'string' ? new Date(msg.createdAt) : msg.createdAt;
            return {
              ...msg,
              createdAt: createdAt,
              text: msg.message,
              sender: msg.senderType.toLowerCase() as 'user' | 'support',
              timestamp: createdAt,
              supportUserName: msg.supportUserName
            };
          })
          .sort((a, b) => {
            const timeA = a.timestamp instanceof Date ? a.timestamp.getTime() : new Date(a.timestamp).getTime();
            const timeB = b.timestamp instanceof Date ? b.timestamp.getTime() : new Date(b.timestamp).getTime();
            return timeA - timeB;
          });
        this.isLoadingMessages = false;
        this.scrollToBottom();
      },
      error: (error) => {
        console.error('Error loading messages', error);
        this.snackbarService.error('خطا در بارگذاری پیام‌ها', 'بستن', 5000);
        this.isLoadingMessages = false;
      }
    });
  }

  startMessagePolling(identifier: string) {
    // توقف polling قبلی
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
    }

    // شروع polling جدید
    const isPhoneNumber = identifier.startsWith('09') && identifier.length === 11;
    this.pollingSubscription = interval(2000)
      .pipe(
        switchMap(() => {
          return isPhoneNumber
            ? this.chatService.getMessagesByPhone(identifier)
            : this.chatService.getMessages(identifier);
        }),
        catchError(error => {
          console.error('Error polling messages', error);
          return of([]);
        })
      )
      .subscribe(messages => {
        const newMessages = messages
          .filter(msg => !this.messages.some(m => m.id === msg.id))
          .map(msg => {
            const createdAt = typeof msg.createdAt === 'string' ? new Date(msg.createdAt) : msg.createdAt;
            return {
              ...msg,
              createdAt: createdAt,
              text: msg.message,
              sender: msg.senderType.toLowerCase() as 'user' | 'support',
              timestamp: createdAt
            };
          });

        if (newMessages.length > 0) {
          this.messages.push(...newMessages);
          this.scrollToBottom();
        }
      });
  }

  sendReply() {
    if (!this.newMessage.trim() || !this.selectedSession) {
      return;
    }

    if (!this.selectedSession.phoneNumber) {
      this.snackbarService.error('شماره موبایل کاربر یافت نشد', 'بستن', 5000);
      return;
    }

    const messageText = this.newMessage.trim();
    this.newMessage = '';

    this.chatService.sendReply({
      message: messageText,
      phoneNumber: this.selectedSession.phoneNumber
    }).subscribe({
      next: (message) => {
        const createdAt = typeof message.createdAt === 'string' ? new Date(message.createdAt) : message.createdAt;
        this.messages.push({
          ...message,
          createdAt: createdAt,
          text: message.message,
          sender: message.senderType.toLowerCase() as 'user' | 'support',
          timestamp: createdAt
        });
        this.scrollToBottom();
        this.snackbarService.success('پاسخ با موفقیت ارسال شد', 'بستن', 3000);
      },
      error: (error) => {
        console.error('Error sending reply', error);
        const errorMessage = error.error?.message || 'خطا در ارسال پاسخ';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
      }
    });
  }

  onKeyPress(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendReply();
    }
  }

  scrollToBottom() {
    setTimeout(() => {
      const chatMessages = document.querySelector('.chat-messages');
      if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
      }
    }, 100);
  }

  formatTime(date: Date | string): string {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    const hours = dateObj.getHours().toString().padStart(2, '0');
    const minutes = dateObj.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  }

  formatDate(date: Date | string): string {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    const now = new Date();
    const diff = now.getTime() - dateObj.getTime();
    const minutes = Math.floor(diff / 60000);

    if (minutes < 1) return 'همین الان';
    if (minutes < 60) return `${minutes} دقیقه پیش`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} ساعت پیش`;
    const days = Math.floor(hours / 24);
    return `${days} روز پیش`;
  }
}
