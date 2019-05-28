import React from 'react';
import ReactDOM from 'react-dom';
import SmidigProsjekt from 'views/SmidigProsjekt';
import StartPage from 'views/StartPage';
import InterestPage from 'views/InterestPage';

// Import all the routeable views into the global window variable.
Object.assign(window, {
    StartPage,
    InterestPage
});

// Hot module replacement.  
if (module.hot) {
    const render = (react, elemId) => {
        ReactDOM.unmountComponentAtNode(document.getElementById(elemId));
        ReactDOM.render(React.createElement(react), document.getElementById(elemId));
    };

    module.hot.accept('./views/SmidigProsjekt.js', _ => render(require('views/SmidigProsjekt').default, 'App'));
    module.hot.accept('./views/StartPage.js', _ => render(require('views/StartPage').default, 'Content'));
    module.hot.accept('./views/InterestPage.js', _ => render(require('views/InterestPage').default, 'Content'));
}

export default SmidigProsjekt;