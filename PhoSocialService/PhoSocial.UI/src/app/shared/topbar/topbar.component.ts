import { Component } from '@angular/core';
import { AuthService } from 'src/app/services/auth.service';
import { Router } from '@angular/router';
import { ChatService } from 'src/app/services/chat.service';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html'
})
export class TopbarComponent {
  userName: string | null = null;
  chatUsers: any[] = [];

  constructor(public auth: AuthService, private router: Router, public chat: ChatService) {
    this.userName = this.auth.getUserNameFromToken();
    this.chat.chatUsers$.subscribe(users => this.chatUsers = users);
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
