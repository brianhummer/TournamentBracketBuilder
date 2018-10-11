import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';
import { TournamentForm } from './components/TournamentForm';
import { TournamentDetails } from './components/TournamentDetails';
import { TournamentsPage } from './components/TournamentsPage';

export default class App extends Component {
    displayName = App.name

    render() {
        return (
            <Layout>
                <Route exact path='/' component={Home} />
                <Route exact path='/tournaments' component={TournamentsPage} />
                <Route path='/tournaments/create' component={TournamentForm} />
                <Route path='/tournaments/details/:id' component={TournamentDetails} />
            </Layout>
        );
    }
}
