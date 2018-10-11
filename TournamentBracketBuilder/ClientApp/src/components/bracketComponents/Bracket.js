import React, { Component, Fragment } from 'react';
import { Row, Col } from 'react-bootstrap';
import { Match } from './Match';

export class Bracket extends Component {
    render() {
        return (
            <Fragment>
                <Row>
                    {this.props.value.map(round =>  
                        <Col key={round.id}>
                            {round.matches.map(match =>
                                <Match key={match} value={this.props.matches[match]} competitors={this.props.competitors} compact></Match>
                        )}
                        </Col>
                    )}
                </Row>
            </Fragment>
        );
    }
}