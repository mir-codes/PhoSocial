import { Component, OnDestroy, OnInit } from "@angular/core";
import { ChatService } from "src/app/services/chat.service";
import { AuthService } from "src/app/services/auth.service";
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

  constructor(private chat: ChatService, private auth: AuthService) { }

  async ngOnInit() {
    await this.chat.start();
    this.sub = this.chat.messages$.subscribe(m => this.messages = m);
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  async send() {
    // NOTE: replace with actual receiver id for testing
    const receiverId = "11111111-2222-3333-4444-555555555555";
    await this.chat.sendMessage(receiverId, this.text);
    this.text = "";
  }
}
