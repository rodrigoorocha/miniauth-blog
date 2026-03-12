import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { PostService } from '../../core/services/post.service';
import { AuthService } from '../../core/services/auth.service';
import { PostDto } from '../../core/models/post.model';
import { PostCreateComponent } from './post-create.component';

@Component({
  selector: 'app-post-list',
  standalone: true,
  imports: [RouterLink, DatePipe, PostCreateComponent],
  templateUrl: './post-list.component.html',
})
export class PostListComponent implements OnInit, OnDestroy {
  // Signal = o "setState" do Angular moderno
  // Quando muda via .set(), o Angular SABE que precisa atualizar a tela
  // Sem Signal (variável normal), o Angular 21 zoneless NÃO detecta a mudança
  posts = signal<PostDto[]>([]);
  loading = signal(true);

  // Controla se o modal de criar post tá aberto ou fechado
  // No React seria: const [showModal, setShowModal] = useState(false)
  showCreateModal = signal(false);

  // Tubo de comunicação: o input de busca joga texto aqui com .next()
  private _searchSubject = new Subject<string>();
  // Recibo da assinatura: guardamos pra cancelar no ngOnDestroy
  private _subscription!: Subscription;

  // Injeta os services (equivale a AddScoped no Program.cs)
  constructor(
    private _postService: PostService,
    private _auth: AuthService
  ) {}

  // Roda UMA VEZ quando o componente aparece na tela (igual useEffect(fn, []) no React)
  ngOnInit(): void {
    // Configura o ouvinte do tubo de busca:
    // Quando o usuário digita algo no input → debounce → busca no backend
    this._subscription = this._searchSubject
      .pipe(
        // Espera 400ms sem digitar nada antes de agir
        debounceTime(400),
        // Se o texto é igual ao anterior, ignora
        distinctUntilChanged(),
        // PASSO 2: Faz GET /api/posts?title=... no backend
        switchMap((title) => this._postService.list(title || undefined))
      )
      // PASSO 3: Backend respondeu → subscribe recebe o resultado
      .subscribe({
        next: (result) => {
          // PASSO 4: .set() avisa o Angular que o valor mudou → atualiza a tela
          this.posts.set(result.data);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        },
      });

    // PASSO 1: Página abriu → busca todos os posts (sem filtro)
    this.loadPosts();
  }

  // Chamado toda vez que o usuário digita no input de busca
  // Joga o texto no tubo → debounceTime espera → switchMap busca
  onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this._searchSubject.next(value);
  }

  // Busca inicial: traz todos os posts sem filtro
  loadPosts(): void {
    this.loading.set(true);
    this._postService.list().subscribe({
      next: (result) => {
        this.posts.set(result.data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  // Abre o modal de criar post
  openCreateModal(): void {
    this.showCreateModal.set(true);
  }

  // Fecha o modal (clicou no X, fundo escuro, ou cancelou)
  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  // Post criado com sucesso → fecha o modal e recarrega a lista
  onPostCreated(): void {
    this.showCreateModal.set(false);
    this.loadPosts();
  }

  logout(): void {
    this._auth.logout();
    window.location.href = '/login';
  }

  // Roda quando o componente SAI da tela (cleanup)
  // Cancela a assinatura do tubo pra não vazar memória
  ngOnDestroy(): void {
    this._subscription?.unsubscribe();
  }
}
