import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private postHub!: signalR.HubConnection;
  private queue: (() => Promise<void>)[] = [];
  private running = false;

  startPostHub(token: string) {
    this.postHub = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7095/hubs/post', { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.postHub.on('PostLiked', (postId: string) => {
      // TODO: Update UI for post like in real-time
    });
    this.postHub.on('PostCommented', (postId: string) => {
      // TODO: Update UI for post comment in real-time
    });

    this.postHub.start();
  }

  likePost(postId: string) {
    this.enqueue(() => this.postHub.invoke('LikePost', postId));
  }

  commentPost(postId: string, content: string) {
    this.enqueue(() => this.postHub.invoke('CommentPost', postId, content));
  }

  enqueue(fn: () => Promise<void>) {
    this.queue.push(fn);
    this.runQueue();
  }

  async runQueue() {
    if (this.running) return;
    this.running = true;
    while (this.queue.length) {
      const fn = this.queue.shift();
      if (fn) await fn();
    }
    this.running = false;
  }
}
