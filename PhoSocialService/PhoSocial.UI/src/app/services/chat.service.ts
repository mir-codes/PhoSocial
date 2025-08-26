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
    });

    this.hubConn.on('UserTyping', (id: string) => this.typing$.next(id));

    await this.hubConn.start();
  }

  async sendMessage(receiverId: string, content: string) {
    if (!this.hubConn) throw new Error('not connected');
    await this.hubConn.invoke('SendMessage', receiverId, content);
  }

  async typing(receiverId: string) {
    if (!this.hubConn) return;
    await this.hubConn.invoke('Typing', receiverId);
  }

  stop() { this.hubConn?.stop(); }
}
