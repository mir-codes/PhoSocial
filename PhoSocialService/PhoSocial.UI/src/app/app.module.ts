import { NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { FormsModule,ReactiveFormsModule } from "@angular/forms";

import { TopbarComponent } from "./shared/topbar/topbar.component";
import { LoginComponent } from "./pages/login/login.component";
import { SignupComponent } from "./pages/signup/signup.component";
import { FeedComponent } from "./pages/feed/feed.component";
import { ChatComponent } from "./pages/chat/chat.component";
import { ProfileComponent } from './pages/profile/profile.component';
import { PostCreateComponent } from './pages/post-create/post-create.component';
import { PostDetailComponent } from './pages/post-detail/post-detail.component';
import { SearchbarComponent } from './shared/searchbar/searchbar.component';
import { UserListComponent } from './shared/user-list/user-list.component';
import { MessageListComponent } from './shared/message-list/message-list.component';
import { MessageInputComponent } from './shared/message-input/message-input.component';
import { FooterComponent } from './shared/footer/footer.component';
import { UserSearchComponent } from './pages/user-search/user-search.component';
import { ThemeToggleComponent } from './shared/theme-toggle/theme-toggle.component';
import { LoadingSpinnerComponent } from './shared/loading-spinner/loading-spinner.component';

@NgModule({
  declarations: [
    AppComponent,
    TopbarComponent,
    LoginComponent,
    SignupComponent,
    FeedComponent,
    ChatComponent,
    ProfileComponent,
    PostCreateComponent,
    PostDetailComponent,
    SearchbarComponent,
    UserListComponent,
    MessageListComponent,
    MessageInputComponent,
    UserSearchComponent,
    FooterComponent,
    ThemeToggleComponent,
    LoadingSpinnerComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule   
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
