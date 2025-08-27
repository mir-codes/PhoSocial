
import { Component, Output, EventEmitter } from '@angular/core';
import { FeedService } from 'src/app/services/feed.service';

@Component({
  selector: 'app-post-create',
  templateUrl: './post-create.component.html',
  styleUrls: ['./post-create.component.css']
})
export class PostCreateComponent {
  caption = '';
  image?: File;
  loading = false;
  error = '';
  @Output() posted = new EventEmitter<void>();

  constructor(private feed: FeedService) {}

  onFileChange(e: any) {
    if (e.target.files && e.target.files.length) {
      this.image = e.target.files[0];
    }
  }

  submit() {
    if (!this.caption) {
      this.error = 'Caption required.';
      return;
    }
    this.loading = true;
    this.feed.createPost(this.caption, this.image).subscribe({
      next: () => {
        this.caption = '';
        this.image = undefined;
        this.loading = false;
        this.error = '';
        this.posted.emit();
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to create post.';
      }
    });
  }
}
