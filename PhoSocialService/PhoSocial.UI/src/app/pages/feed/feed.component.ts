import { Component, OnInit } from '@angular/core';
import { FeedService } from 'src/app/services/feed.service';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-feed',
  templateUrl: './feed.component.html',
  styleUrls: ['./feed.component.css']
})
export class FeedComponent implements OnInit {
  posts: any[] = [];
  filteredPosts: any[] = [];
  showCreate = false;
  isLoggedIn = false;
  likedPosts: Set<string> = new Set();

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
    this.feed.getPosts().subscribe(res => { 
      this.posts = res; 
      this.filteredPosts = res; 
    });
  }

  reloadPosts() { this.load(); this.showCreate = false; }
  search(q: string) { this.filteredPosts = this.posts.filter(x => (x.caption || '').toLowerCase().includes(q.toLowerCase())); }
  openCreate() { this.showCreate = !this.showCreate; }
  
  toggleLike(postId: string) {
    if (this.likedPosts.has(postId)) {
      this.likedPosts.delete(postId);
    } else {
      this.likedPosts.add(postId);
    }
  }

  openComments(postId: string) {
    // TODO: Navigate to post detail or open comment modal
    console.log('Opening comments for post:', postId);
  }

  isLiked(postId: string): boolean {
    return this.likedPosts.has(postId);
  }

  like(id: string) {
    if (!this.isLoggedIn) {
      window.location.href = '/login';
      return;
    }
    this.likedPosts.add(id);
    this.feed.like(id).subscribe(
      () => this.load(),
      () => this.likedPosts.delete(id)
    );
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
