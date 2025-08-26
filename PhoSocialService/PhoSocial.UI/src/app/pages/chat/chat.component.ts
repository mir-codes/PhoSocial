import { Component, OnDestroy, OnInit } from "@angular/core";
import { ChatService } from "src/app/services/chat.service";
import { AuthService } from "src/app/services/auth.service";
import { ActivatedRoute } from '@angular/router';
import { Subscription } from "rxjs";

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
  loading = true;

  constructor(private chat: ChatService, private auth: AuthService, private route: ActivatedRoute) { }

  async ngOnInit() {
    await this.chat.start();
    this.sub = this.chat.messages$.subscribe(m => this.messages = m);
    this.route.queryParams.subscribe(params => {
      this.receiverId = params['userId'] || '';
      if (this.receiverId) {
        this.loadHistory();
      }
    });
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  loadHistory() {
    // Fetch chat history from API (ChatController)
    this.loading = true;
    // You may want to move this to ChatService for better separation
    fetch(`/api/Chat/history/${this.receiverId}`, {
      headers: { 'Authorization': `Bearer ${this.auth.getToken()}` }
    })
      .then(res => res.json())
      .then(data => {
        this.messages = data || [];
        this.loading = false;
      })
      .catch(() => this.loading = false);
  }

  async send() {
    if (!this.receiverId) return;
    await this.chat.sendMessage(this.receiverId, this.text);
    this.text = "";
  }
}
