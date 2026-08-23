import { Link } from 'react-router-dom';
import type { ReactNode } from 'react';

interface AuthLayoutProps {
  title: string;
  subtitle: string;
  children: ReactNode;
  footerText: string;
  footerLinkText: string;
  footerLinkTo: string;
}

export function AuthLayout({
  title,
  subtitle,
  children,
  footerText,
  footerLinkText,
  footerLinkTo,
}: AuthLayoutProps) {
  return (
    <div className="page-center">
      <div className="card auth-card">
        <div className="brand">
          <span className="brand-badge">43</span>
          <div>
            <h1>{title}</h1>
            <p>{subtitle}</p>
          </div>
        </div>
        {children}
        <p className="muted footer-link">
          {footerText}{' '}
          <Link to={footerLinkTo}>{footerLinkText}</Link>
        </p>
      </div>
    </div>
  );
}
