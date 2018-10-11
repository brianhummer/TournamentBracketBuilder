import React, { Component } from 'react';
import { Button, Jumbotron, Container } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';

export class Home extends Component {
  displayName = Home.name

  render() {
    return (
        <div>
            <Jumbotron fluid>
                <Container>
                    <h1>Fluid jumbotron</h1>
                    <p>
                        This is a modified jumbotron that occupies the entire horizontal space of
                        its parent.
                    </p>
                </Container>
            </Jumbotron>
            <LinkContainer to={'/tournaments/create'}>
                <Button variant="primary">New Tournament</Button>
            </LinkContainer>
      </div>
    );
  }
}
