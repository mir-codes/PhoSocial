import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthService } from './auth.service';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ChatService {
  public hubConn!: signalR.HubConnection;
  public messages$ = new BehaviorSubject<any[]>([]); // newest first
  public typing$ = new BehaviorSubject<any | null>(null);
  public chatUsers$ = new BehaviorSubject<any[]>([]);

  constructor(private auth: AuthService, private http: HttpClient) {}

  async start() {
    const token = this.auth.getToken();
    if (!token) throw new Error('no token');

    this.hubConn = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.hubUrl}/chat`, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.hubConn.on('ReceiveMessage', (msg: any) => {
      const cur = this.messages$.value;
      this.messages$.next([msg, ...cur]);
      this.updateChatUsersFromMessage(msg);
    });

    this.hubConn.on('MessageSent', (msg: any) => {
      const cur = this.messages$.value;
      this.messages$.next([msg, ...cur]);
      this.updateChatUsersFromMessage(msg);
    });

    this.hubConn.on('UserTyping', (payload: any) => this.typing$.next(payload));

    await this.hubConn.start();
  }

  async sendMessage(otherUserId: string | number, content: string) {
    if (!this.hubConn) throw new Error('not connected');
    const otherId = typeof otherUserId === 'string' ? parseInt(otherUserId, 10) : otherUserId;
    await this.hubConn.invoke('SendMessage', otherId, content);
  }

  async typing(otherUserId: string | number) {
    if (!this.hubConn) return;
    const otherId = typeof otherUserId === 'string' ? parseInt(otherUserId, 10) : otherUserId;
    await this.hubConn.invoke('Typing', otherId);
  }

  stop() { this.hubConn?.stop(); }

  // HTTP helpers
  getConversations() {
    return this.http.get<any[]>(`${environment.apiUrl}/v2/chat/conversations`);
  }

  getMessages(conversationId: number, offset = 0, pageSize = 20) {
    return this.http.get<any[]>(`${environment.apiUrl}/v2/chat/messages/${conversationId}?offset=${offset}&pageSize=${pageSize}`);
  }

  getOrCreateConversation(otherUserId: number) {
    return this.http.post<any>(`${environment.apiUrl}/v2/chat/conversations/with/${otherUserId}`, {});
  }

  async updateChatUsersFromMessage(msg: any) {
    const myId = this.auth.getUserIdFromToken();
    const otherId = msg.senderId === myId ? msg.receiverId : msg.senderId;
    const otherName = msg.username || msg.senderName || otherId;
    const chatUsers = [...this.chatUsers$.value];
    let user = chatUsers.find((u) => u.id === otherId);
    if (!user) {
      user = { id: otherId, name: otherName, unread: 0, lastMessage: '', lastTime: '' };
      chatUsers.push(user);
    }
    user.name = otherName;
    user.lastMessage = msg.messageText || msg.content || '';
    user.lastTime = msg.createdAt || new Date().toISOString();
    if (msg.senderId !== myId) {
      user.unread = (user.unread || 0) + 1;
    }
    chatUsers.sort((a, b) => (b.lastTime > a.lastTime ? 1 : -1));
    this.chatUsers$.next(chatUsers);
  }
}
