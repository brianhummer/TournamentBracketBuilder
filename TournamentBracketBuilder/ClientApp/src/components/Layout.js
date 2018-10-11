import React, { Component } from 'react';
import { Container } from 'react-bootstrap';
import { NavMenu } from './NavMenu';

export class Layout extends Component {
    displayName = Layout.name

    render() {
        return (
            <div>
                <NavMenu />
                <Container className="mt-2">
                    {this.props.children}
                </Container>
            </div>
        );
    }
}