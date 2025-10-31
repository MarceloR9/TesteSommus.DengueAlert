import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

// A interface WeekQuery não é mais necessária para buscar as últimas semanas,
// mas a mantemos aqui por consistência se for usada em outro lugar.
export interface WeekQuery { ew: number; ey: number; }

// DTO adaptado para receber a estrutura que o novo Controller retorna
export interface DengueDto {
  semana_epidemiologica: string;
  casos_est?: number | null;
  casos_notificados?: number | null;
  nivel_alerta?: number | null;
  error?: string | null; // Adicionado null para maior flexibilidade
}

@Injectable({
  providedIn: 'root'
})
export class DengueService {
  constructor(private http: HttpClient) {}

  /**
   * Busca dados de alerta para uma SE específica (EW/EY).
   */
  getWeek(ew: number, ey: number): Observable<DengueDto> {
    const url = `/api/dengue/semana?ew=${ew}&ey=${ey}`;
    return this.http.get<DengueDto>(url).pipe(
      catchError(err => {
        const message = err?.error?.message || err.statusText || 'Erro desconhecido';
        return of({
          semana_epidemiologica: `${ey}-${String(ew).padStart(2, '0')}`,
          error: message
        } as DengueDto);
      })
    );
  }

  // A LÓGICA DE CÁLCULO DE DATA NO FRONTEND (getLastNWeeks) FOI REMOVIDA.
  // Todo o cálculo de qual é a última SE completa agora é feito no Backend (AlertDengueService.cs).


  /**
   * Busca as últimas 'n' Semanas Epidemiológicas COMPLETAS.
   * Chama o novo endpoint do Backend que garante o retorno de semanas já encerradas.
   */
  getUltimasSemanasCompletas(n: number = 3): Observable<DengueDto[]> {
    // CHAVE DA CORREÇÃO: Chamada direta ao novo endpoint do Controller
    const url = `/api/dengue/ultimas-semanas/${n}`;

    return this.http.get<DengueDto[]>(url).pipe(
        catchError(err => {
            console.error('Erro ao buscar as últimas semanas completas:', err);
            // Retorna um Observable com uma lista vazia ou erro
            return of([{
                semana_epidemiologica: 'Erro',
                error: 'Falha na comunicação com o servidor.'
            } as DengueDto]);
        })
    );
  }

  /**
   * Método de conveniência para buscar as últimas 3 semanas.
   * Chama a função corrigida.
   */
  getLastThree(): Observable<DengueDto[]> {
    return this.getUltimasSemanasCompletas(3);
  }
}
