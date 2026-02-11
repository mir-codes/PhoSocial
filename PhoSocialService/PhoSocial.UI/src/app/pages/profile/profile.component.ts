import { Component, OnInit } from '@angular/core';
import { AuthService } from 'src/app/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  userName = '';
  userEmail = '';

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit() {
    this.loadUserInfo();
  }

  loadUserInfo() {
    // Get user info from AuthService methods
    this.userName = this.auth.getUserNameFromToken() || 'User';
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
