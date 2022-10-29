import React from 'react';
import { Link } from "react-router-dom";
import styles from './navigation.module.css';

function Navigation() {
    return (
        <>
            <Link to="/" className={styles.item}>Notifications</Link>
            <Link to="/alerts" className={styles.item}>Alerts</Link>
        </>
    );
}

export default Navigation;
