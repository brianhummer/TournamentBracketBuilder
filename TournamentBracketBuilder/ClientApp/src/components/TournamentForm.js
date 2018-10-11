import React, { Component, Fragment } from 'react';
import { Form, Button, Col } from 'react-bootstrap';
import { Redirect } from 'react-router-dom';

export class TournamentForm extends Component {

    constructor(props) {
        super(props);

        this.state = {
            redirect: false,
            isLoading: false,
            name: "",
            isDoubleElim: false,
            competitors: [{ name: '', seed: '0' }]
        };

        this.handleNameChange = this.handleNameChange.bind(this);
        this.handleDoubleElimChange = this.handleDoubleElimChange.bind(this);
        this.onSubmit = this.onSubmit.bind(this);
    }

    onSubmit(e) {
        e.preventDefault();

        fetch('api/Tournaments', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                name: this.state.name,
                competitors: this.state.competitors,
                isDoubleElim: this.state.isDoubleElim
            })
        })
            .then(data => {
                this.setState({ redirect: true });
        });

        
    }

    handleNameChange(e) {
        this.setState({ name: e.target.value });
    }

    handleCompetitorNameChange(index, e) {
        const newCompetitors = this.state.competitors.map((competitor, sIndex) => {
            if (index !== sIndex) return competitor;
            return { ...competitor, name: e.target.value };
        });

        this.setState({ competitors: newCompetitors });
    }

    handleCompetitorSeedChange(index, e) {
        const newCompetitors = this.state.competitors.map((competitor, sIndex) => {
            if (index !== sIndex) return competitor;
            return { ...competitor, seed: e.target.value };
        });

        this.setState({ competitors: newCompetitors });
    }

    handleAddCompetitor() {
        this.setState({ competitors: this.state.competitors.concat([{ name: '', seed: '0' }]) });
    }

    handleRemoveCompetitor(index) {
        const newCompetitors = this.state.competitors.filter((s, sidx) => index !== sidx)
            .map((competitor) => {
                if (competitor.seed < this.state.competitors.length) return competitor;
                return { ...competitor, seed: (competitor.seed - 1).toString() };
        });


        this.setState({ competitors: newCompetitors });
    }

    handleDoubleElimChange(e) {
        this.setState({ isDoubleElim: e.target.checked });
    }

    render() {
        const { redirect } = this.state;

        if (redirect) {
            return <Redirect to='/tournaments' />;
        }

        return (
            <Fragment>
                <h1>Create Tournament</h1>
                <Form onSubmit={this.onSubmit}>
                    <Form.Group controlId="tournamentName">
                        <Form.Label>Tournament Name</Form.Label>
                        <Form.Control placeholder="Tournament name" value={this.state.name} onChange={this.handleNameChange} />
                    </Form.Group>
                    <Form.Group id="formBasicChecbox">
                        <Form.Check label="Double elmination" checked={this.state.checkboxChecked} onChange={this.handleDoubleElimChange}/>
                    </Form.Group>
                    <Form.Group className="mb-0">
                            <Form.Label>Competitor</Form.Label>
                    </Form.Group>
                    {this.state.competitors.map((competitor, index) => (
                        <div key={index} className="competitor">
                            <Form.Row>
                                <Form.Group as={Col} sm={8} controlId={"competitor-" + index}>
                                    <Form.Control placeholder={"Competitor " + (index + 1)} value={competitor.name} onChange={(e) => this.handleCompetitorNameChange(index, e)}/>
                                </Form.Group>
                                <Form.Group as={Col} sm={2} controlId={"seed-" + index}>
                                    <Form.Control as="select" value={competitor.seed} onChange={(e) => this.handleCompetitorSeedChange(index, e)}>
                                        <option value='0'>Not seeded</option>
                                        {this.state.competitors.map((competitor, index) => (
                                            <option key={index} value={(index + 1)}>{(index + 1)}</option>
                                        ))}
                                    </Form.Control>
                                </Form.Group>
                                <Form.Group as={Col} sm={2}>
                                    <Button variant="danger" onClick={() => this.handleRemoveCompetitor(index)}>Remove</Button>
                                </Form.Group>
                            </Form.Row>
                        </div>
                    ))}
                    <Form.Group>
                        <Button variant="primary" onClick={() => this.handleAddCompetitor()}>Add Competitor</Button>
                    </Form.Group>
                    <Button variant="primary" type="submit">
                        Submit
                    </Button>
                </Form>
            </Fragment>
        );
    }
}
