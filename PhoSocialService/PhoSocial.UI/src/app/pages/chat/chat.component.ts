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
  parallelQueue: (() => Promise<void>)[] = [];
  running = false;

  constructor(
    private chat: ChatService,
    private auth: AuthService,
    private route: ActivatedRoute,
    private search: SearchService
  ) { }

  async ngOnInit() {
    await this.chat.start();
    this.sub = this.chat.messages$.subscribe(m => this.messages = m);
    this.route.queryParams.subscribe(params => {
      this.receiverId = params['userId'] || '';
      if (this.receiverId) {
        this.loadReceiverName();
        this.loadHistory();
      }
    });
    // Real-time receive
    if (this.chat.hubConn) {
      this.chat.hubConn.on('ReceiveMessage', (msg: any) => {
        this.enqueue(() => this.handleIncomingMessage(msg));
      });
    }
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  loadReceiverName() {
    this.search.users('').subscribe(users => {
      const user = users.find((u: any) => u.id === this.receiverId);
      this.receiverName = user ? user.userName : this.receiverId;
    });
  }

  loadHistory() {
    this.loading = true;
    fetch(`/api/Chat/history/${this.receiverId}`, {
      headers: { 'Authorization': `Bearer ${this.auth.getToken()}` }
    })
      .then(res => res.json())
      .then(data => {
        this.messages = (data || []).map((msg: any) => ({
          ...msg,
          senderName: msg.senderName || msg.senderId
        }));
        this.loading = false;
      })
      .catch(() => this.loading = false);
  }

  async send() {
    if (!this.receiverId) return;
    await this.chat.sendMessage(this.receiverId, this.text);
    this.text = "";
  }

  enqueue(fn: () => Promise<void>) {
    this.parallelQueue.push(fn);
    this.runQueue();
  }

  async runQueue() {
    if (this.running) return;
    this.running = true;
    while (this.parallelQueue.length) {
      const fn = this.parallelQueue.shift();
      if (fn) await fn();
    }
    this.running = false;
  }

  async handleIncomingMessage(msg: any) {
    this.messages = [{
      ...msg,
      senderName: msg.senderName || msg.senderId
    }, ...this.messages];
  }
}
