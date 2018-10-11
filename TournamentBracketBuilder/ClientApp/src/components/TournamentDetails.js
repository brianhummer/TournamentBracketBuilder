import React, { Component, Fragment } from 'react';
import { Nav } from 'react-bootstrap';
import { Competitors } from './bracketComponents/Competitors';
import { Bracket } from './bracketComponents/Bracket';
import { RoundPage } from './RoundPage';

export class TournamentDetails extends Component {

    constructor(props) {
        super(props);
        this.state = {
            activePage: "overview",
            tournamentName: "",
            isDoubleElim: false,
            competitors: [],
            rounds: [],
            matches: {},
            winner: "",
            loading: true
        };

        this.renderTournamentDetails = this.renderTournamentDetails.bind(this);
        this.handleGameChange = this.handleGameChange.bind(this);
        this.handleRoundSubmit = this.handleRoundSubmit.bind(this);
    }

    calculateWinner(rounds, matches, competitors) {
        let winner = "";

        if (rounds[rounds.length - 1].isComplete) {
            let lastMatch = matches[rounds[rounds.length - 1].matches[rounds[rounds.length - 1].matches.length - 1]];
            if (lastMatch.winner !== null) {
                if (parseInt(lastMatch.winner, 10) === 0) {
                    winner = competitors[lastMatch.competitorOneId].name;
                }
                else {
                    winner = competitors[lastMatch.competitorTwoId].name;
                }
            }
        }

        return winner;
    }

    handleRoundSubmit(id) {
        fetch('api/Rounds/Complete/' + id, {
            method: 'PUT'
        })
        .then(response => response.json())
        .then(data => {
            console.log(data);
            const newMatches = Object.assign({}, this.state.matches);
            const newRounds = this.state.rounds.slice();
            let currRoundIndex = -1;

            for (var i = 0; i < newRounds.length; i++) {
                newRounds[i].matches = newRounds[i].matches.slice();
                if (currRoundIndex === -1 && newRounds[i].isComplete === false) {
                    currRoundIndex = i;
                }
            }

            newRounds[currRoundIndex].isComplete = data[0].isComplete;

            for (i = 0; i < data.length; i++) {
                if (newRounds.length >= parseInt(data[i].roundNumber, 10)) {
                    for (var j = 0; j < data[i].matches.length; j++) {
                        if (data[i].matches[j].nextWinnersMatch) {
                            newMatches[data[i].matches[j].nextWinnersMatch.id] = data[i].matches[j].nextWinnersMatch;
                        }
                        if (data[i].matches[j].nextLosersMatch) {
                            newMatches[data[i].matches[j].nextLosersMatch.id] = data[i].matches[j].nextLosersMatch;
                        }
                    }
                }
                else {
                    const round = {
                        id: data[i].id,
                        roundNumber: data[i].roundNumber,
                        roundType: data[i].roundType,
                        name: data[i].name,
                        matches: [],
                        isComplete: data[i].isComplete
                    };

                    for (var k = 0; k < data[i].matches.length; k++) {
                        newMatches[data[i].matches[k].id] = data[i].matches[k];
                        round.matches.push(data[i].matches[k].id);
                    }

                    newRounds.push(round);
                }
            }

            let winner = this.calculateWinner(newRounds, newMatches, this.state.competitors);
            let activePage = winner === '' ? 'play' : 'overview';

            this.setState({
                rounds: newRounds,
                matches: newMatches,
                winner: winner,
                activePage: activePage
            });
        });
    }

    handleGameChange(id, result) {
        const newMatches = Object.assign({}, this.state.matches);
        newMatches[id].winner = result;
        fetch('api/Matches/' + id, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(newMatches[id])
        })
        .then(response => {
            if (response.status === 204)
            {
                this.setState({ matches: newMatches });
            } 
        });
    }

    componentDidMount() {
        fetch('api/Tournaments/' + this.props.match.params.id)
            .then(response => response.json())
            .then(data => {

                const rounds = [];
                const matches = {};
                const competitors = {};

                for (var i = 0; i < data.rounds.length; i++) {
                    const round = {
                        id: data.rounds[i].id,
                        roundNumber: data.rounds[i].roundNumber,
                        roundType: data.rounds[i].roundType,
                        name: data.rounds[i].name,
                        matches: [],
                        isComplete: data.rounds[i].isComplete
                    };

                    for (var j = 0; j < data.rounds[i].matches.length; j++) {
                        matches[data.rounds[i].matches[j].id] = data.rounds[i].matches[j];
                        round.matches.push(data.rounds[i].matches[j].id);
                    }

                    rounds.push(round);
                }

                for (i = 0; i < data.competitors.length; i++) {
                    competitors[data.competitors[i].id] = data.competitors[i];
                }

                this.setState({
                    loading: false,
                    tournamentName: data.name,
                    isDoubleElim: data.isDoubleElim,
                    competitors: competitors,
                    rounds: rounds,
                    matches: matches,
                    winner: this.calculateWinner(rounds, matches, competitors)
                });
            });
    }

    renderTournamentDetails() {
        let activeContent;
        let winnersRounds = [];
        let losersRounds = [];
        let grandFinals = [];
        let reset = [];

        for (var i = 0; i < this.state.rounds.length; i++) {
            switch (this.state.rounds[i].roundType) {
                case 0: // winners
                    winnersRounds.push(this.state.rounds[i]);
                    break;
                case 1: // losers
                    losersRounds.push(this.state.rounds[i]);
                    break;
                case 2: // grand finals
                    grandFinals.push(this.state.rounds[i]);
                    break;
                case 3: // grand finals reset
                    reset.push(this.state.rounds[i]);
                    break;
                default:
                    break;
            }                
        }

        if (this.state.activePage === "overview") {
            activeContent =
                <Fragment>
                    {this.state.winner !== '' &&
                        <h1>{this.state.winner} is the {this.state.tournamentName} champion!</h1>
                    }
                    <h3>Winners Bracket</h3>
                    <Bracket value={winnersRounds} matches={this.state.matches} competitors={this.state.competitors}></Bracket>
                    {losersRounds.length > 0 &&
                        <Fragment>
                            <h3 className="mt-3">Losers Bracket</h3>
                            <Bracket value={losersRounds} matches={this.state.matches} competitors={this.state.competitors}></Bracket>
                        </Fragment>
                    }
                    {grandFinals.length > 0 &&
                        <Fragment>
                            <h3 className="mt-3">Grand Finals</h3>
                            <Bracket value={grandFinals} matches={this.state.matches} competitors={this.state.competitors}></Bracket>
                        </Fragment>
                    }
                    {reset.length > 0 &&
                        <Fragment>
                            <h3 className="mt-3">Grand Finals Reset</h3>
                            <Bracket value={reset} matches={this.state.matches} competitors={this.state.competitors}></Bracket>
                        </Fragment>
                    }
                    <h3 className="mt-3">Competitors</h3>
                    <Competitors value={this.state.competitors}></Competitors>
                </Fragment>;
        }
        else {
            let currRound;
            for (i = 0; i < this.state.rounds.length; i++) {
                if (this.state.rounds[i].isComplete === false) {
                    currRound = this.state.rounds[i];
                    break;
                }
            }

            activeContent =
                <RoundPage
                    value={currRound}
                    matches={this.state.matches}
                    competitors={this.state.competitors}
                    onSubmit={this.handleRoundSubmit}
                    onChange={this.handleGameChange}
                />;
        }
    
        return (
            <div>
                {activeContent}
            </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderTournamentDetails();
        return (
            <Fragment>
                <h1>{this.state.tournamentName ? this.state.tournamentName : 'Untitled Tournament'}</h1>
                <Nav variant="pills" activeKey={this.state.activePage} onSelect={(selectedKey) => this.setState({ activePage: selectedKey })}>
                    <Nav.Item>
                        <Nav.Link eventKey="overview">Overview</Nav.Link>
                    </Nav.Item>
                    <Nav.Item>
                        {this.state.winner === '' ?
                            <Nav.Link eventKey="play">Play</Nav.Link> :
                            <Nav.Link disabled eventKey="play">Play</Nav.Link>
                        }
                    </Nav.Item>
                </Nav>
                {contents}
            </Fragment>
        );
    }
}