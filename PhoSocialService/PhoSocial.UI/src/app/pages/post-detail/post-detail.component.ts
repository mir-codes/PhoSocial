import { Component, OnInit } from '@angular/core';
// TODO: Inject FeedService, ActivatedRoute, and implement fetching post/comments by ID
@Component({
  selector: 'app-post-detail',
  templateUrl: './post-detail.component.html',
  styleUrls: ['./post-detail.component.scss']
})
export class PostDetailComponent implements OnInit {
  post: any;
  comments: any[] = [];
  loading = true;

  ngOnInit() {
    // TODO: Fetch post and comments by route param
    // Example: this.route.snapshot.paramMap.get('id')
    // Use FeedService to get post and comments
    // Set loading = false when done
  }
}
