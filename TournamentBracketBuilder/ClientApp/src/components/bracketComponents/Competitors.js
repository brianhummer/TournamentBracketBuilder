import React, { Component } from 'react';
import { Table, Nav } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';

export class Competitors extends Component {
    render() {
        return (
            <Table responsive>
                <thead>
                    <tr>
                        <th>Competitor</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    {Object.keys(this.props.value).map((key, index) =>
                        <tr key={key}>
                            <td><LinkContainer to={'/competitor/details/' + key}><Nav.Link>{this.props.value[key].name ? this.props.value[key].name : 'unnamed competitor'}</Nav.Link></LinkContainer></td>
                            <td>Elimintated</td>
                        </tr>
                    )}
                </tbody>
            </Table>
        );
    }
}