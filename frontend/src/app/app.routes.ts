import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login.component';
import { RegisterComponent } from './features/auth/register.component';
import { PostListComponent } from './features/posts/post-list.component';
import { PostDetailComponent } from './features/posts/post-detail.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'posts/:id', component: PostDetailComponent, canActivate: [authGuard] },
  { path: 'posts', component: PostListComponent, canActivate: [authGuard] },
  { path: '', redirectTo: 'posts', pathMatch: 'full' },
];
