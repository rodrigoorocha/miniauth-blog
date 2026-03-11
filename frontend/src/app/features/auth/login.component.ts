import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  form: FormGroup;
  errorMessage = '';
  loading = false;

  constructor(
    private _fb: FormBuilder,
    private _auth: AuthService,
    private _router: Router
  ) {
    this.form = this._fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.loading = true;
    this.errorMessage = '';

    this._auth.login(this.form.value).subscribe({
      next: () => {
        this._router.navigate(['/']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Email ou senha inválidos';
      },
    });
  }
}
