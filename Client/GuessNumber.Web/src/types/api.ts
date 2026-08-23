export interface User {
  id: string;
  email: string;
  bestScore: number | null;
}

export interface StartGameResponse {
  gameId: string;
  min: number;
  max: number;
}

export interface GuessResponse {
  result: 'higher' | 'lower' | 'correct';
  attempts: number;
  completed: boolean;
  bestScore?: number | null;
}

export interface HintResponse {
  hint: string;
  hintsUsed: number;
}

export interface ApiError {
  error: string;
}
