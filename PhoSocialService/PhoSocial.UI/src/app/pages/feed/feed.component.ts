import { Component, OnInit } from '@angular/core';
import { FeedService } from 'src/app/services/feed.service';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  templateUrl: './feed.component.html'
})
export class FeedComponent implements OnInit {
  posts: any[] = [];
  filteredPosts: any[] = [];
  showCreate = false;
  isLoggedIn = false;

  constructor(private feed: FeedService, private route: ActivatedRoute, private auth: AuthService) {}

  ngOnInit() {
    this.isLoggedIn = this.auth.isLoggedIn();
    this.load();
    this.route.queryParams.subscribe(p => {
      const q = p['q'] || '';
      if (q) this.search(q);
    });
  }

  load() {
    this.feed.getPosts().subscribe(res => { this.posts = res; this.filteredPosts = res; });
  }

  reloadPosts() { this.load(); this.showCreate = false; }
  search(q: string) { this.filteredPosts = this.posts.filter(x => (x.caption || '').toLowerCase().includes(q.toLowerCase())); }
  openCreate() { this.showCreate = !this.showCreate; }
  like(id: string) {
    if (!this.isLoggedIn) {
      window.location.href = '/login';
      return;
    }
    this.feed.like(id).subscribe(()=> this.load());
  }
  comment(postId: string, content: string) {
    if (!this.isLoggedIn) {
      window.location.href = '/login';
      return;
    }
    this.feed.comment(postId, content).subscribe(()=> this.load());
  }
  openDetail(post: any) { /* navigate to detail or open modal — implement as needed */ }
}
