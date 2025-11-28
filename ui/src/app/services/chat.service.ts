import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface ChatMessage {
  id: number;
  message: string;
  userId?: number;
  supportUserId?: number;
  phoneNumber?: string;
  chatSessionId: string;
  isRead: boolean;
  senderType: 'User' | 'Support';
  senderName?: string;
  supportUserName?: string;
  createdAt: Date | string;
}

export interface ChatSession {
  chatSessionId: string;
  phoneNumber?: string;
  userName?: string;
  userId?: number;
  unreadCount: number;
  lastMessageTime: Date | string;
  lastMessage?: string;
}

export interface CreateChatMessage {
  message: string;
  phoneNumber: string;
}

export interface SupportReply {
  message: string;
  phoneNumber: string;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService extends BaseApiService {
  constructor(protected override http: HttpClient) {
    super(http);
    this.baseUrl = `${environment.apiUrl}/chat`;
  }

  sendMessage(dto: CreateChatMessage): Observable<ChatMessage> {
    return this.post<ChatMessage>('send', dto);
  }

  sendReply(dto: SupportReply): Observable<ChatMessage> {
    return this.post<ChatMessage>('reply', dto);
  }

  getMessages(chatSessionId: string): Observable<ChatMessage[]> {
    return this.get<ChatMessage[]>(`messages/${chatSessionId}`);
  }

  getMessagesByPhone(phoneNumber: string): Observable<ChatMessage[]> {
    return this.get<ChatMessage[]>(`messages/phone/${phoneNumber}`);
  }

  getMessagesByPhoneForSupport(phoneNumber: string): Observable<ChatMessage[]> {
    return this.get<ChatMessage[]>(`messages/phone/${phoneNumber}/support`);
  }

  getActiveSessions(phoneNumber?: string): Observable<ChatSession[]> {
    const params = phoneNumber ? `?phoneNumber=${phoneNumber}` : '';
    return this.get<ChatSession[]>(`sessions${params}`);
  }

  markAsRead(chatSessionId: string): Observable<void> {
    return this.post<void>(`mark-read/${chatSessionId}`, {});
  }

  getSupportStatus(): Observable<{ isOnline: boolean; onlineCount: number }> {
    return this.get<{ isOnline: boolean; onlineCount: number }>('support-status');
  }

  getUnansweredCount(): Observable<number> {
    return this.get<number>('unanswered-count');
  }

  // تولید شناسه جلسه چت بر اساس شماره موبایل
  generateChatSessionId(phoneNumber: string): string {
    return `chat-${phoneNumber}`;
  }
}
