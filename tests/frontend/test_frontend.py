#!/usr/bin/env python
# -*- coding: utf-8 -*-

import unittest
from random import randint
from time import sleep

class TestStringMethods(unittest.TestCase):

    def test_1(self):
        # Test
        sleep(randint(1,3))
        self.assertTrue(True,"This should be true")

    def test_2(self):
        # Test
        sleep(randint(1,3))
        self.assertTrue(True,"This should be true")

    def test_3(self):
        # Test
        sleep(randint(1,3))
        self.assertTrue(True,"This should be true")                

if __name__ == '__main__':
    unittest.main()