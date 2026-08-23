import api from './client';
import type { GuessResponse, HintResponse, StartGameResponse } from '../types/api';

export async function startGame(): Promise<StartGameResponse> {
  const response = await api.post<StartGameResponse>('/api/game/start');
  return response.data;
}

export async function submitGuess(gameId: string, guess: number): Promise<GuessResponse> {
  const response = await api.post<GuessResponse>(`/api/game/${gameId}/guess`, { guess });
  return response.data;
}

export async function requestHint(gameId: string): Promise<HintResponse> {
  const response = await api.post<HintResponse>(`/api/game/${gameId}/hint`);
  return response.data;
}
