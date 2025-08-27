import { Component } from '@angular/core';
import { SearchService } from 'src/app/services/search.service';

@Component({
  selector: 'app-user-search',
  templateUrl: './user-search.component.html',
  styleUrls: ['./user-search.component.css']
})
export class UserSearchComponent {
  term = '';
  users: any[] = [];
  loading = false;
  error = '';

  constructor(private search: SearchService) {}

  doSearch() {
    if (!this.term.trim()) {
      this.users = [];
      this.error = '';
      return;
    }
    this.loading = true;
    this.search.users(this.term).subscribe({
      next: (res) => {
        this.users = res;
        this.loading = false;
        this.error = '';
      },
      error: () => {
        this.loading = false;
        this.error = 'Search failed.';
      }
    });
  }
}
