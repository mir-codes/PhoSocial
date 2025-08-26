import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FeedService } from 'src/app/services/feed.service';
@Component({
  selector: 'app-post-detail',
  templateUrl: './post-detail.component.html',
  styleUrls: ['./post-detail.component.scss']
})
export class PostDetailComponent implements OnInit {
  post: any;
  comments: any[] = [];
  loading = true;
  commentText = '';
  error = '';

  constructor(private route: ActivatedRoute, private feed: FeedService) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loading = true;
      this.feed.getPosts().subscribe({
        next: posts => {
          this.post = posts.find((p: any) => p.id === id);
          // TODO: Replace with API call for comments if available
          this.comments = this.post?.comments || [];
          this.loading = false;
        },
        error: () => this.loading = false
      });
    }
  }

  like() {
    if (!this.post?.id) return;
    this.feed.like(this.post.id).subscribe(()=>{
      // Optionally update like count
    });
  }

  addComment() {
    if (!this.commentText.trim() || !this.post?.id) return;
    this.feed.comment(this.post.id, this.commentText).subscribe({
      next: () => {
        this.comments.push({ userName: 'You', content: this.commentText });
        this.commentText = '';
        this.error = '';
      },
      error: () => this.error = 'Failed to add comment.'
    });
  }
}
