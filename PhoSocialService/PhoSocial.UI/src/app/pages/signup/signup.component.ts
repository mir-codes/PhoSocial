import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html'
})
export class SignupComponent {
  userName = '';
  email = '';
  password = '';
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  onSignup() {
    this.auth.signup(this.userName, this.email, this.password).subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => this.error = 'Signup failed. Try again.'
    });
  }
}
