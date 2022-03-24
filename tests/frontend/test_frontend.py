#!/usr/bin/env python
# -*- coding: utf-8 -*-

import pytest
from random import randint
from time import sleep

def test_1():
    # Test
    sleep(randint(1,3))
    assert True

def test_2():
    # Test
    sleep(randint(1,3))
    assert True

def test_3():
    # Test
    sleep(randint(1,3))
    assert True

if __name__ == '__main__':
    pytest.main()