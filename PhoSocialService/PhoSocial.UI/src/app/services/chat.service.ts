import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ChatService {
  public hubConn!: signalR.HubConnection;
  public messages$ = new BehaviorSubject<any[]>([]);
  public typing$ = new BehaviorSubject<string | null>(null);
  public chatUsers$ = new BehaviorSubject<any[]>([]); // { id, name, unread, lastMessage, lastTime }

  constructor(private auth: AuthService) {}

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
      this.updateChatUsers(msg);
    });

    this.hubConn.on('UserTyping', (id: string) => this.typing$.next(id));

    await this.hubConn.start();
  }

  async sendMessage(receiverId: string, content: string) {
    if (!this.hubConn) throw new Error('not connected');
    await this.hubConn.invoke('SendMessage', receiverId, content);
    // Add to chat list immediately
    this.updateChatUsers({ senderId: this.auth.getUserIdFromToken(), receiverId, content, time: new Date().toISOString() });
  }

  async typing(receiverId: string) {
    if (!this.hubConn) return;
    await this.hubConn.invoke('Typing', receiverId);
  }

  stop() { this.hubConn?.stop(); }

  updateChatUsers(msg: any) {
    // msg: { senderId, receiverId, content, time, senderName }
    const userId = this.auth.getUserIdFromToken();
    const otherId = msg.senderId === userId ? msg.receiverId : msg.senderId;
    const otherName = msg.senderName || msg.receiverName || otherId;
    const chatUsers = [...this.chatUsers$.value];
    let user = chatUsers.find(u => u.id === otherId);
    if (!user) {
      user = { id: otherId, name: otherName, unread: 0, lastMessage: '', lastTime: '' };
      chatUsers.push(user);
    }
    user.name = otherName;
    user.lastMessage = msg.content;
    user.lastTime = msg.time || new Date().toISOString();
    if (msg.senderId !== userId) {
      user.unread = (user.unread || 0) + 1;
    }
    // Sort by lastTime desc
    chatUsers.sort((a, b) => (b.lastTime > a.lastTime ? 1 : -1));
    this.chatUsers$.next(chatUsers);
  }
}
