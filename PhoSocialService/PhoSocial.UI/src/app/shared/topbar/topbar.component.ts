import { Component } from '@angular/core';
import { AuthService } from 'src/app/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html'
})
export class TopbarComponent {
  userName: string | null = null;

  constructor(public auth: AuthService, private router: Router) {
    this.userName = this.auth.getUserNameFromToken();
  }

  get isLoggedIn() {
    return this.auth.isLoggedIn();
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
    this.userName = null;
  }

  onSearch(q: string) {
    this.router.navigate(['/feed'], { queryParams: { q } });
  }
}
