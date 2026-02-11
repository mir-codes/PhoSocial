import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FeedService } from 'src/app/services/feed.service';
@Component({
  selector: 'app-post-detail',
  templateUrl: './post-detail.component.html',
  styleUrls: ['./post-detail.component.css']
})
export class PostDetailComponent implements OnInit {
  post: any;
  comments: any[] = [];
  loading = true;
  commentText = '';
  error = '';
  isLiked = false;
  likedPosts: Set<string> = new Set();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private feed: FeedService
  ) {}

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
    this.isLiked = !this.isLiked;
    this.feed.like(this.post.id).subscribe(
      () => {
        if (this.isLiked) {
          this.post.likeCount = (this.post.likeCount || 0) + 1;
        } else {
          this.post.likeCount = Math.max(0, (this.post.likeCount || 1) - 1);
        }
      },
      () => {
        this.isLiked = !this.isLiked;
      }
    );
  }

  addComment() {
    if (!this.commentText.trim() || !this.post?.id) return;
    this.feed.comment(this.post.id, this.commentText).subscribe({
      next: () => {
        this.comments.push({
          userName: 'You',
          content: this.commentText,
          createdAt: new Date()
        });
        this.commentText = '';
        this.error = '';
      },
      error: () => this.error = 'Failed to add comment.'
    });
  }

  goBack() {
    this.router.navigate(['/feed']);
  }
}
