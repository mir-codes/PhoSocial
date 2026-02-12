// ...existing code...
import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { ChatService } from "src/app/services/chat.service";
import { AuthService } from "src/app/services/auth.service";
import { ActivatedRoute } from '@angular/router';
import { Subscription } from "rxjs";
import { SearchService } from "src/app/services/search.service";

@Component({
  selector: "app-chat",
  templateUrl: "./chat.component.html",
  styleUrls: ["./chat.component.css"]
})
export class ChatComponent implements OnInit, OnDestroy {
  messages: any[] = []; // newest first
  text = "";
  sub!: Subscription;
  receiverId = '';
  receiverName = '';
  loading = true;
  chatUsers: any[] = [];
  selectedUserId = '';
  chatUsersSub!: Subscription;
  audio = new Audio('/assets/NotificationSounds/MessageRecived.mp3');
    sending = false;
  conversationId: number | null = null;
  pageSize = 20;
  loadingMore = false;
  @ViewChild('messagesContainer') messagesContainer!: ElementRef;

  constructor(
    private chat: ChatService,
    private auth: AuthService,
    private route: ActivatedRoute,
    private search: SearchService
  ) { }

  async ngOnInit() {
    await this.chat.start();
    this.sub = this.chat.messages$.subscribe(m => {
      this.messages = m;
      // play sound for incoming
      if (m.length && m[0].senderId !== this.auth.getUserIdFromToken()) {
        try { this.audio.play(); } catch {}
      }
      // scroll to bottom for new messages
      setTimeout(() => this.scrollToBottom(), 50);
    });
    this.chatUsersSub = this.chat.chatUsers$.subscribe(users => this.chatUsers = users);
    // load conversation list
    this.chat.getConversations().subscribe(list => this.chatUsers = list || []);
    this.route.queryParams.subscribe(params => {
      this.receiverId = params['userId'] || '';
      if (this.receiverId) {
        this.selectedUserId = this.receiverId;
        this.loadReceiverName();
        this.openConversationWithUser(+this.receiverId);
      }
    });
  }

  loadReceiverName() {
    this.search.users('').subscribe(users => {
      const user = users.find((u: any) => u.id === this.receiverId);
      this.receiverName = user ? user.userName : this.receiverId;
    });
  }

  async openConversationWithUser(otherUserId: number) {
    this.loading = true;
    try {
      const conv = await this.chat.getOrCreateConversation(otherUserId).toPromise();
      this.conversationId = conv?.conversationId ?? null;
      this.selectedUserId = String(otherUserId);
      await this.loadMessages(true);
      // reset unread locally
      const user = this.chatUsers.find(u => u.otherUserId === otherUserId || u.id === otherUserId || u.otherUserId === Number(this.selectedUserId));
      if (user) user.unread = 0;
    } catch (err) {
      console.error('failed open conversation', err);
    } finally {
      this.loading = false;
    }
  }

  async loadMessages(initial = false) {
    if (!this.conversationId) return;
    if (initial) this.messages = [];
    const offset = this.messages.length;
    this.loadingMore = true;
    try {
      const res = await this.chat.getMessages(this.conversationId, offset, this.pageSize).toPromise();
      // API returns newest first; we want newest first in array too
      this.messages = [...this.messages, ...res];
      // if initial load, scroll to bottom
      if (initial) setTimeout(() => this.scrollToBottom(), 50);
    } catch (err) {
      console.error('failed load messages', err);
    } finally {
      this.loadingMore = false;
    }
  }

  async send() {
      if (!this.selectedUserId || !this.text.trim() || this.sending) return;
      this.sending = true;
      try {
        // Ensure SignalR connection is started
        if (!this.chat.hubConn || this.chat.hubConn.state !== 'Connected') {
          await this.chat.start();
        }
        await this.chat.sendMessage(this.selectedUserId, this.text);
        this.text = "";
        // optimistic: messages$ will be updated via hub event
      } catch (err) {
        console.error('Failed to send message:', err);
        // Optionally show error to user
      } finally {
        this.sending = false;
      }
  }

  // Called by template when message scroll reaches top -> load older
  async onMessagesScroll(ev: any) {
    const el = ev.target as HTMLElement;
    if (el.scrollTop === 0 && !this.loadingMore) {
      await this.loadMessages(false);
    }
  }

  private scrollToBottom() {
    try {
      const el = this.messagesContainer?.nativeElement as HTMLElement;
      if (el) el.scrollTop = el.scrollHeight;
    } catch {}
  }

  onSelectUser(id: string) {
    this.selectedUserId = id;
    this.receiverId = id;
    this.loadReceiverName();
    this.openConversationWithUser(+id);
    this.text = "";
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
