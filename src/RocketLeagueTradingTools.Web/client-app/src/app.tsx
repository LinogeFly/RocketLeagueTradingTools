import React, {Component} from 'react';
import {Route, Switch} from 'react-router-dom';
import AlertsPage from './components/pages/alertsPage';
import NotFoundPage from './components/pages/notFoundPage';
import NotificationsPage from './components/pages/notificationsPage';
import Navigation from './components/navigation';
import {config} from './services/config';

export default class App extends Component {
    componentDidMount(): void {
        document.title = config.defaultTitle;
    }

    render() {
        return (
            <div className="rltt-page">
                <div className="rltt-container text-center">
                    <nav className="mb-4">
                        <Navigation/>
                    </nav>
                </div>
                <div className="rltt-container">
                    <main>
                        <Switch>
                            <Route exact path="/" component={NotificationsPage}/>
                            <Route exact path="/alerts" component={AlertsPage}/>
                            <Route component={NotFoundPage}/>
                        </Switch>
                    </main>
                </div>
            </div>
        );
    }
}
