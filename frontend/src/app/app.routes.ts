import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'posts/:id',
    loadComponent: () =>
      import('./features/posts/post-detail.component').then((m) => m.PostDetailComponent),
    canActivate: [authGuard],
  },
  {
    path: 'posts',
    loadComponent: () =>
      import('./features/posts/post-list.component').then((m) => m.PostListComponent),
    canActivate: [authGuard],
  },
  { path: '', redirectTo: 'posts', pathMatch: 'full' },
];
