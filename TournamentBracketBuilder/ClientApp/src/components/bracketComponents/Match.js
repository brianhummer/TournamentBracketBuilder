import React, { Component, Fragment } from 'react';
import { Card, Form, Row, Col } from 'react-bootstrap';

export class Match extends Component {

    constructor(props) {
        super(props);
        this.handleGameChange = this.handleGameChange.bind(this);
        this.formatPlayerLabel = this.formatPlayerLabel.bind(this);
    }

    handleGameChange(e) {
        this.props.onChange(this.props.value.id, e.target.value);
    }

    formatPlayerLabel(competitorId) {
        let label;

        if (competitorId) {
            if (this.props.value.winner === null) {
                label = <span>
                    {this.props.competitors[competitorId].name}({this.props.competitors[competitorId].seed})
                </span>;
            }
            else if ((parseInt(this.props.value.winner, 10) === 0 && this.props.value.competitorOneId === competitorId) || (parseInt(this.props.value.winner, 10) === 1 && this.props.value.competitorTwoId === competitorId)) {
                label = <span className="font-weight-bold">
                    {this.props.competitors[competitorId].name}
                    <span className="text-muted">({this.props.competitors[competitorId].seed})</span>
                </span>;
            }
            else {
                label = <del>
                    <span>{this.props.competitors[competitorId].name}</span>
                    <span className="text-muted">({this.props.competitors[competitorId].seed})</span>
                </del>;
            }
        }
        else {
            label = <span>TBD</span>;
        }

        return label;
    }

    render() {
        let playerOne = this.formatPlayerLabel(this.props.value.competitorOneId);
        let playerTwo = this.formatPlayerLabel(this.props.value.competitorTwoId);
        let form;

        if (!this.props.compact) {
            form = <Card.Body>
                {this.props.value.competitorOneId && this.props.value.competitorTwoId ?
                    <Form>
                        <Form.Group as={Row} controlId="tournamentName">
                            <Form.Label column sm={3}>Game 1</Form.Label>
                            <Col sm={9}>
                                <Form.Control as="select" value={this.props.value.winner !== null ? '' + this.props.value.winner : ''} onChange={this.handleGameChange}>
                                    <option value=''>Not played</option>
                                    <option value='0'>{this.props.competitors[this.props.value.competitorOneId].name}</option>
                                    <option value='1'>{this.props.competitors[this.props.value.competitorTwoId].name}</option>
                                </Form.Control>
                            </Col>
                        </Form.Group>
                    </Form> : 'Results TBD'
                }
            </Card.Body>;
        }

        return (
            <Fragment>
                <Card bg="light">
                    <Card.Header>
                        {playerOne}
                        <span> vs </span>
                        {playerTwo}
                        <span className="float-right text-secondary">Match {this.props.value.matchNumber}</span>
                    </Card.Header>
                    {form}
                </Card>
            </Fragment>
        );
    }
}