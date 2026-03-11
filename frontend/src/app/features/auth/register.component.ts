import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  form: FormGroup;
  errorMessage = '';
  loading = false;

  constructor(
    private _fb: FormBuilder,
    private _auth: AuthService,
    private _router: Router
  ) {
    this.form = this._fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.loading = true;
    this.errorMessage = '';

    this._auth.register(this.form.value).subscribe({
      next: () => {
        this._router.navigate(['/']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Erro ao criar conta';
      },
    });
  }
}
