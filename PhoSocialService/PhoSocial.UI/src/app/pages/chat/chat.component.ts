// ...existing code...
import { Component, OnDestroy, OnInit } from "@angular/core";
import { ChatService } from "src/app/services/chat.service";
import { AuthService } from "src/app/services/auth.service";
import { ActivatedRoute } from '@angular/router';
import { Subscription } from "rxjs";
import { SearchService } from "src/app/services/search.service";

@Component({
  selector: "app-chat",
  templateUrl: "./chat.component.html",
  styleUrls: ["./chat.component.scss"]
})
export class ChatComponent implements OnInit, OnDestroy {
  messages: any[] = [];
  text = "";
  sub!: Subscription;
  receiverId = '';
  receiverName = '';
  loading = true;
  chatUsers: any[] = [];
  selectedUserId = '';
  chatUsersSub!: Subscription;
  audio = new Audio('/assets/MessageRecived.mp3');

  constructor(
    private chat: ChatService,
    private auth: AuthService,
    private route: ActivatedRoute,
    private search: SearchService
  ) { }

  async ngOnInit() {
    await this.chat.start();
    this.sub = this.chat.messages$.subscribe(m => this.messages = m);
    this.chatUsersSub = this.chat.chatUsers$.subscribe(users => this.chatUsers = users);
    this.route.queryParams.subscribe(params => {
      this.receiverId = params['userId'] || '';
      if (this.receiverId) {
        this.selectedUserId = this.receiverId;
        this.loadReceiverName();
        this.loadHistory();
      }
    });
  }

  loadReceiverName() {
    this.search.users('').subscribe(users => {
      const user = users.find((u: any) => u.id === this.receiverId);
      this.receiverName = user ? user.userName : this.receiverId;
    });
  }

  loadHistory() {
    this.loading = true;
    fetch(`/api/Chat/history/${this.selectedUserId}`, {
      headers: { 'Authorization': `Bearer ${this.auth.getToken()}` }
    })
      .then(res => res.json())
      .then(data => {
        this.messages = (data || []).map((msg: any) => ({
          ...msg,
          senderName: msg.UserName || msg.senderName || msg.senderId
        }));
        this.loading = false;
        // Reset unread count for this user
        const user = this.chatUsers.find(u => u.id === this.selectedUserId);
        if (user) user.unread = 0;
      })
      .catch(() => this.loading = false);
  }

  async send() {
    if (!this.selectedUserId) return;
    await this.chat.sendMessage(this.selectedUserId, this.text);
    this.text = "";
    this.loadHistory();
  }

  onSelectUser(id: string) {
    this.selectedUserId = id;
    this.receiverId = id;
    this.loadReceiverName();
    this.loadHistory();
  }

  async handleIncomingMessage(msg: any) {
    this.messages = [{
      ...msg,
      senderName: msg.UserName || msg.senderName || msg.senderId
    }, ...this.messages];
    this.audio.play();
  }
  ngOnDestroy() {
    this.sub?.unsubscribe();
    this.chatUsersSub?.unsubscribe();
  }
}
