import React, { Component } from 'react';
import { Route, Switch } from 'react-router-dom';
import AlertsPage from './components/pages/alertsPage';
import NotFoundPage from './components/pages/notFoundPage';
import NotificationsPage from './components/pages/notificationsPage';
import styles from './app.module.css';
import Navigation from './components/navigation';

export default class App extends Component {
  render() {
    return (
      <div className={styles.app}>
        <nav className="mb-4">
          <Navigation />
        </nav>
        <main className={styles.content}>
          <Switch>
            <Route exact path="/" component={NotificationsPage} />
            <Route exact path="/alerts" component={AlertsPage} />
            <Route component={NotFoundPage} />
          </Switch>
        </main>
      </div>
    );
  }
}
