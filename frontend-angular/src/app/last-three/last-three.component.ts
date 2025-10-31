import { Component, OnInit } from '@angular/core';
import { DengueService, DengueDto } from '../dengue.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-last-three',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './last-three.component.html',
  styleUrls: ['./last-three.component.css']
})
export class LastThreeComponent implements OnInit {
  weeks: DengueDto[] | null = null;
  loading: boolean = false;
  error?: string | null = null;
  showData: boolean = false;

  constructor(private dengueService: DengueService) {}

  ngOnInit(): void {

  }

  toggleVisibility(): void {
    // 1. Inverte o estado de visibilidade
    this.showData = !this.showData;

    // 2. Se a transição for para MOSTRAR DADOS
    if (this.showData) {
      // 3. Se os dados ainda não foram carregados, chame load()
      if (this.weeks === null) {
        this.load();
      }
    } else {
      // 4. Se a transição for para ESCONDER DADOS, limpamos a mensagem de erro
      this.error = null;
    }
  }

  // Função de carga, agora chamada por toggleVisibility (1a vez) e pelo botão "Atualizar"
  load(): void {
    this.loading = true;
    this.error = null;
    // Não definimos weeks como null para não piscar os dados enquanto atualiza

    this.dengueService.getLastThree().subscribe({
      next: (data) => {
        this.weeks = data;
        this.loading = false;

        const allErrors = data.every(w => w.error);
        if (allErrors) {
            this.error = "Dados indisponíveis para o período solicitado.";
        } else {
            this.error = null; // Limpa o erro, caso tenha sido setado antes
        }
      },
      error: (err) => {
        this.error = 'Falha ao buscar dados do servidor.';
        this.loading = false;
        this.weeks = []; // Em caso de falha total, limpa a lista
        console.error('Erro ao carregar dados:', err);
      }
    });
  }
}
