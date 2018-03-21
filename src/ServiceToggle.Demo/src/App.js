import React, { Component } from 'react';
import './App.css';

class App extends Component {
  constructor(props) {
    super(props);

    this.state = {
      text: 'loading'
    };
  }

  componentDidMount() {
    fetch('http://localhost:5000/api/value')
      .then(response => response.json())
      .then(json => this.setState({ text: json }));
  }

  render() {
    return (
      <div className="App">
        <header className="App-header">
          <h1 className="App-title">{this.state.text}</h1>
        </header>
      </div>
    );
  }
}

export default App;
