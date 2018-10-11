import React, { Component, Fragment } from 'react';
import { Row, Col, Button } from 'react-bootstrap';
import { Match } from './bracketComponents/Match';

export class RoundPage extends Component {
    render() {
        let nextRoundActive = true;
        let button;

        for (var i = 0; i < this.props.value.matches.length; i++) {
            if (this.props.matches[this.props.value.matches[i]].winner === null || this.props.matches[this.props.value.matches[i]].winner === '') {
                nextRoundActive = false;
                break;
            }
        }

        if (nextRoundActive === true) {
            button = <Button variant="primary" onClick={() => this.props.onSubmit(this.props.value.id)}>Next Round</Button>
        }
        else {
            button = <Button disabled variant="primary" onClick={() => this.props.onSubmit(this.props.value.id)}>Next Round</Button>
        }

        

        return (
            <Fragment>
                <h3>{this.props.value.name ? this.props.value.name : "Current Round"}</h3>
                <Row>
                    {this.props.value.matches.map(match =>
                        <Col className='mb-3' sm={12} md={6} lg={4} key={match}>
                            <Match
                                value={this.props.matches[match]}
                                competitors={this.props.competitors}
                                onChange={(match, result) => this.props.onChange(match, result)}
                            />
                        </Col>
                    )}
                </Row>
                <Row>
                    <Col>
                        {button}
                    </Col>
                </Row>
            </Fragment>
        );
    }
}