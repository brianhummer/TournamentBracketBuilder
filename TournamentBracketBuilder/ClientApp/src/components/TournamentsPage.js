import React, { Component, Fragment } from 'react';
import { Table, Button, Nav } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';

export class TournamentsPage extends Component {

    constructor(props) {
        super(props);
        this.state = { tournaments: [], loading: true };

        fetch('api/Tournaments')
            .then(response => response.json())
            .then(data => {
                this.setState({ tournaments: data, loading: false });
            });
    }

    static renderTournamentsTable(tournaments) {
        return (
            <Table responsive>
                <thead>
                    <tr>
                        <th>Tournament</th>
                        <th>Competitors</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    {tournaments.map(tournament =>
                        <tr key={tournament.id}>
                            <td><LinkContainer to={'/tournaments/details/' + tournament.id}><Nav.Link>{tournament.name ? tournament.name : 'Untitled Tournament'}</Nav.Link></LinkContainer></td>
                            <td>{tournament.competitors.length}</td>
                            <td>Active</td>
                        </tr>
                    )}
                </tbody>
            </Table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : TournamentsPage.renderTournamentsTable(this.state.tournaments);

        return (
            <Fragment>
                <h1>Tournaments</h1>
                <LinkContainer to={'/tournaments/create'}>
                    <Button className="mb-3" variant="primary">Create Tournament</Button>
                </LinkContainer>
                {contents}
            </Fragment>
        );
    }
}