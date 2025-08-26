import { Component, OnInit } from '@angular/core';
import { FeedService } from 'src/app/services/feed.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  templateUrl: './feed.component.html'
})
export class FeedComponent implements OnInit {
  posts: any[] = [];
  filteredPosts: any[] = [];
  showCreate = false;

  constructor(private feed: FeedService, private route: ActivatedRoute) {}

  ngOnInit() {
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
  like(id: string) { this.feed.like(id).subscribe(()=> this.load()); }
  openDetail(post: any) { /* navigate to detail or open modal — implement as needed */ }
}
