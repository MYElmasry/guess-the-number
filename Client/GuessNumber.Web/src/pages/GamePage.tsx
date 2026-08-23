import { type FormEvent, useEffect, useRef, useState } from 'react';
import { requestHint, startGame, submitGuess } from '../api/game';
import { getErrorMessage } from '../api/client';
import { useAuth } from '../context/AuthContext';

export function GamePage() {
  const { user, logout, updateBestScore } = useAuth();
  const guessInputRef = useRef<HTMLInputElement>(null);
  const [gameId, setGameId] = useState<string | null>(null);
  const [min, setMin] = useState(1);
  const [max, setMax] = useState(43);
  const [guess, setGuess] = useState('');
  const [attempts, setAttempts] = useState(0);
  const [feedback, setFeedback] = useState('');
  const [completed, setCompleted] = useState(false);
  const [isNewBest, setIsNewBest] = useState(false);
  const [hint, setHint] = useState('');
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [hintLoading, setHintLoading] = useState(false);
  const [error, setError] = useState('');

  function focusGuessInput() {
    window.setTimeout(() => {
      guessInputRef.current?.focus();
    }, 0);
  }

  useEffect(() => {
    void beginGame();
  }, []);

  useEffect(() => {
    if (!loading && !completed && gameId) {
      focusGuessInput();
    }
  }, [loading, completed, gameId]);

  async function beginGame() {
    setLoading(true);
    setError('');
    setFeedback('');
    setHint('');
    setCompleted(false);
    setIsNewBest(false);
    setAttempts(0);
    setGuess('');

    try {
      const game = await startGame();
      setGameId(game.gameId);
      setMin(game.min);
      setMax(game.max);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  async function handleGuess(event: FormEvent) {
    event.preventDefault();
    if (!gameId || completed) {
      return;
    }

    const parsedGuess = Number(guess);
    if (!Number.isInteger(parsedGuess) || parsedGuess < min || parsedGuess > max) {
      setError(`Enter a whole number between ${min} and ${max}.`);
      focusGuessInput();
      return;
    }

    setSubmitting(true);
    setError('');
    let shouldRefocus = true;

    try {
      const response = await submitGuess(gameId, parsedGuess);
      setAttempts(response.attempts);
      setGuess('');

      if (response.result === 'correct') {
        shouldRefocus = false;
        setCompleted(true);
        setFeedback(`Correct! You got it in ${response.attempts} guesses.`);
        const previousBest = user?.bestScore ?? null;
        const newBest = response.bestScore ?? response.attempts;
        setIsNewBest(previousBest === null || newBest < previousBest);
        if (response.bestScore != null) {
          updateBestScore(response.bestScore);
        }
      } else {
        setFeedback(response.result === 'higher' ? 'Try higher ↑' : 'Try lower ↓');
      }
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setSubmitting(false);
      if (shouldRefocus) {
        focusGuessInput();
      }
    }
  }

  async function handleHint() {
    if (!gameId || completed) {
      return;
    }

    setHintLoading(true);
    setError('');

    try {
      const response = await requestHint(gameId);
      setHint(response.hint);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setHintLoading(false);
    }
  }

  const bestScoreMessage =
    user?.bestScore != null
      ? `Your best score: ${user.bestScore} guesses`
      : "You haven't completed a game yet.";

  return (
    <div className="page-center">
      <div className="card game-card">
        <header className="game-header">
          <div>
            <p className="eyebrow">Guess The Number</p>
            <h1>Welcome, {user?.email}</h1>
            <p className="muted">{bestScoreMessage}</p>
          </div>
          <button type="button" className="secondary" onClick={() => void logout()}>
            Logout
          </button>
        </header>

        <section className="game-panel">
          <p className="lead">I'm thinking of a number between {min} and {max}.</p>

          {loading ? (
            <p>Starting a new game...</p>
          ) : (
            <>
              <form className="guess-form" onSubmit={handleGuess}>
                <label>
                  Your guess
                  <input
                    ref={guessInputRef}
                    type="number"
                    min={min}
                    max={max}
                    value={guess}
                    onChange={(event) => setGuess(event.target.value)}
                    disabled={completed || submitting}
                    required
                  />
                </label>
                <button type="submit" disabled={completed || submitting || !gameId}>
                  {submitting ? 'Checking...' : 'Guess'}
                </button>
              </form>

              <div className="status-row">
                <span>Attempts: {attempts}</span>
                {attempts >= 3 && !completed && (
                  <button type="button" className="secondary" disabled={hintLoading} onClick={() => void handleHint()}>
                    {hintLoading ? 'Getting hint...' : 'Get hint'}
                  </button>
                )}
              </div>

              {feedback && (
                <p className={completed ? 'success' : 'feedback'}>
                  {completed ? `🎉 ${feedback}` : feedback}
                </p>
              )}

              {completed && isNewBest && <p className="success">🏆 New best score!</p>}
              {hint && <p className="hint">Hint: {hint}</p>}
              {error && <p className="error">{error}</p>}

              {completed && (
                <button type="button" onClick={() => void beginGame()}>
                  Play Again
                </button>
              )}
            </>
          )}
        </section>
      </div>
    </div>
  );
}
