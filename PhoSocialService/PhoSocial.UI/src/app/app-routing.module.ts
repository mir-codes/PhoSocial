import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { LoginComponent } from "./pages/login/login.component";
import { SignupComponent } from "./pages/signup/signup.component";
import { FeedComponent } from "./pages/feed/feed.component";
import { ChatComponent } from "./pages/chat/chat.component";
import { ProfileComponent } from "./pages/profile/profile.component"; // ✅ correct path
import { PostDetailComponent } from "./pages/post-detail/post-detail.component"; // ✅ correct path

const routes: Routes = [
  { path: '', redirectTo: 'feed', pathMatch: 'full' },
  { path: 'feed', component: FeedComponent },
  { path: 'chat', component: ChatComponent },
  { path: 'login', component: LoginComponent },
  { path: 'signup', component: SignupComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'post/:id', component: PostDetailComponent },
  { path: '**', redirectTo: 'feed' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
